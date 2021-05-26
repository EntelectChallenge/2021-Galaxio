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
            var stop2 = Stopwatch.StartNew();
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
                        Logger.LogInfo("Core", "Waiting for all bots to connect");
                    }

                    Thread.Sleep(1000);
                    continue;
                }

                stop2.Restart();
                await ProcessGameTick();
                Logger.LogDebug("RunLoop", $"Processing tick took {stop2.ElapsedMilliseconds}ms");

                stop2.Restart();
                var gameStateDto = worldStateService.GetPublishedState();
                await hubConnection.InvokeAsync("PublishGameState", gameStateDto);
                Logger.LogDebug("RunLoop", $"Published game state, Time: {stop2.ElapsedMilliseconds}");

                Logger.LogDebug("RunLoop", "Waiting for Tick Ack");
                stop2.Restart();
                while (TickAcked != worldStateService.GetState().World.CurrentTick)
                {
                }

                Logger.LogDebug("RunLoop", $"TickAck matches current tick, Time: {stop2.ElapsedMilliseconds}");

                if (stopwatch.ElapsedMilliseconds < engineConfig.TickRate)
                {
                    var delay = (int) (engineConfig.TickRate - stopwatch.ElapsedMilliseconds);
                    if (delay > 0)
                    {
                        Thread.Sleep(delay);
                    }
                }

                Logger.LogInfo("TIMER", $"Game Loop Time: {stopwatch.ElapsedMilliseconds}ms");
                stopwatch.Restart();
            } while (!HasWinner &&
                hubConnection.State == HubConnectionState.Connected);

            if (!HasWinner &&
                hubConnection.State != HubConnectionState.Connected)
            {
                Logger.LogError("RunLoop", "Runner disconnected before a winner was found");
                throw new InvalidOperationException("Runner disconnected before a winner was found");
            }

            await hubConnection.InvokeAsync("GameComplete", worldStateService.GenerateGameCompletePayload());
        }

        private async Task ProcessGameTick()
        {
            Logger.LogInfo(
                "Engine",
                $"Tick: {worldStateService.GetState().World.CurrentTick}, Player Count: {worldStateService.GetPlayerCount()}");
            IList<BotObject> bots = worldStateService.GetPlayerBots();

            var stoplog = new StopWatchLogger();
            SimulateTickForBots(bots);
            stoplog.Log("Simulation complete");

            IList<BotObject> aliveBots = worldStateService.GetPlayerBots();
            IEnumerable<BotObject> botsForRemoval = bots.Where(bot => !aliveBots.Contains(bot));
            foreach (var bot in botsForRemoval)
            {
                await hubConnection.InvokeAsync("PlayerConsumed", bot.Id);
            }

            stoplog.Log("Informed Consumed Bots");

            worldStateService.ApplyAfterTickStateChanges();
            stoplog.Log("After Tick SC Complete");
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