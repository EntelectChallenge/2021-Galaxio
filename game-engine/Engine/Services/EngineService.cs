using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Models;
using Domain.Services;
using Engine.Interfaces;
using Engine.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Engine.Services
{
    public class EngineService : IEngineService
    {
        private readonly EngineConfig engineConfig;
        private readonly IActionService actionService;
        private readonly IWorldStateService worldStateService;
        private readonly ITickProcessingService tickProcessingService;
        private HubConnection hubConnection;
        public int TickAcked { get; set; }
        public bool HasWinner { get; set; }
        public bool PendingStart { get; set; }
        public bool GameStarted { get; set; }

        public EngineService(
            IWorldStateService worldStateService,
            IActionService actionService,
            IConfigurationService engineConfig,
            ITickProcessingService tickProcessingService)
        {
            this.worldStateService = worldStateService;
            this.actionService = actionService;
            this.tickProcessingService = tickProcessingService;
            this.engineConfig = engineConfig.Value;
        }

        public HubConnection SetHubConnection(ref HubConnection connection) => hubConnection = connection;

        public async Task GameRunLoop()
        {
            var stopwatch = Stopwatch.StartNew();
            do
            {
                if (hubConnection.State != HubConnectionState.Connected)
                {
                    break;
                }

                if (!GameStarted)
                {
                    if (!PendingStart)
                    {
                        Logger.LogInfo("RunLoop", "Waiting for all bots to connect");
                    }

                    Thread.Sleep(1000);
                    continue;
                }

                if (stopwatch.ElapsedMilliseconds < engineConfig.TickRate)
                {
                    var delay = (int) (engineConfig.TickRate - stopwatch.ElapsedMilliseconds);
                    if (delay > 0)
                    {
                        Thread.Sleep(delay);
                    }
                }

                Logger.LogInfo("RunLoop", $"Game Loop Time: {stopwatch.ElapsedMilliseconds} milliseconds");
                stopwatch.Restart();

                Logger.LogDebug("Engine.ConnectionState", hubConnection.State);
                var elapsedPreTick = stopwatch.ElapsedMilliseconds;
                await ProcessGameTick();
                Logger.LogDebug("RunLoop", $"Processing tick took {stopwatch.ElapsedMilliseconds - elapsedPreTick}ms");
                await hubConnection.InvokeAsync("PublishGameState", worldStateService.GetPublishedState());
                var elapsedTime = stopwatch.ElapsedMilliseconds;
                while (TickAcked != worldStateService.GetState().World.CurrentTick)
                {
                    continue;
                }
                Logger.LogDebug("RunLoop", $"Waited {stopwatch.ElapsedMilliseconds - elapsedTime}ms for TickAck");

            } while (!HasWinner && hubConnection.State == HubConnectionState.Connected);

            if (!HasWinner && hubConnection.State != HubConnectionState.Connected)
            {
                Logger.LogError("GameRunLoop", "Runner disconnected before a winner was found");
                throw new InvalidOperationException("Runner disconnected before a winner was found");
            }

            await hubConnection.InvokeAsync("GameComplete", worldStateService.GenerateGameCompletePayload());
        }

        private async Task ProcessGameTick()
        {
            Logger.LogInfo("Engine", $"Tick: {worldStateService.GetState().World.CurrentTick}, Player Count: {worldStateService.GetPlayerCount()}");
            IList<BotObject> bots = worldStateService.GetPlayerBots();

            SimulateTickForBots(bots);

            IList<BotObject> aliveBots = worldStateService.GetPlayerBots();
            IEnumerable<BotObject> botsForRemoval = bots.Where(bot => !aliveBots.Contains(bot));
            foreach (var bot in botsForRemoval)
            {
                await hubConnection.InvokeAsync("PlayerConsumed", bot.Id);
            }

            foreach (var bot in aliveBots)
            {
                Logger.LogDebug(bot.Id, "Size", bot.Size);
                Logger.LogDebug(bot.Id, "Speed", bot.Speed);
                Logger.LogDebug(bot.Id, "Position", $"{bot.Position.X}:{bot.Position.Y}");
            }

            worldStateService.ApplyAfterTickStateChanges();
            CheckWinConditions();
        }

        public void SimulateTickForBots(IList<BotObject> bots)
        {
            foreach (var bot in bots)
            {
                actionService.ApplyActionToBot(bot);
            }

            tickProcessingService.SimulateTick();
        }

        private void CheckWinConditions()
        {
            if (worldStateService.GetState().World.CurrentTick >= engineConfig.MaxRounds)
            {
                worldStateService.FinalisePlayerPlacements();
                HasWinner = true;
                Logger.LogInfo("WinCondition", $"Max Rounds Reached! Winning Bot: {worldStateService.GetPlayerBots().First().Id}");
            }

            if (worldStateService.GetPlayerCount() > 1)
            {
                return;
            }

            worldStateService.FinalisePlayerPlacements();
            HasWinner = true;
            Logger.LogInfo("WinCondition", $"We have a winner! Bot {worldStateService.GetPlayerBots().First().Id}");
        }
    }
}