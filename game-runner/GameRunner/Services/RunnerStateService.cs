using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Domain.Models;
using Domain.Services;
using GameRunner.Interfaces;
using GameRunner.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace GameRunner.Services
{
    public class RunnerStateService : IRunnerStateService
    {
        private StateObject gameEngine;
        private StateObject gameLogger;
        private Timer componentTimer;
        private readonly RunnerConfig runnerConfig;

        private readonly ConcurrentDictionary<Guid, string> ActiveConnections = new ConcurrentDictionary<Guid, string>();
        private readonly ConcurrentDictionary<Guid, string> AllConnections = new ConcurrentDictionary<Guid, string>();
        private readonly ConcurrentDictionary<Guid, string> RegistrationTokens = new ConcurrentDictionary<Guid, string>();
        private readonly ConcurrentDictionary<Guid, string> NickNames = new ConcurrentDictionary<Guid, string>();
        public List<Guid> BotActionsReceived = new List<Guid>();

        private GameCompletePayload gameCompletePayload;
        private readonly IHostApplicationLifetime applicationLifetime;

        public int TotalConnectedClients => ActiveConnections.Count;
        public int TotalConnectedBots => RegistrationTokens.Count;
        public int TotalConnections => AllConnections.Count;
        public string FailureReason { get; set; }
        public bool IsCoreReady => GetEngine() != default && GetLogger() != default;
        public GameCompletePayload GameCompletePayload
        {
            get => gameCompletePayload;
            set
            {
                gameCompletePayload = value;
                gameCompletePayload.Players.ForEach(player => player.Nickname = NickNames[Guid.Parse(player.Id)]);
            }
        }

        public RunnerStateService(IConfigurationService runnerConfig, IHostApplicationLifetime appLifetime)
        {
            this.runnerConfig = runnerConfig.RunnerConfig;
            applicationLifetime = appLifetime;
        }

        public StateObject GetEngine() => gameEngine;

        public StateObject GetLogger() => gameLogger;
        public ConcurrentDictionary<Guid, string> GetActiveConnections() => ActiveConnections;

        public ConcurrentDictionary<Guid, string> GetRegistrationTokens() => RegistrationTokens;

        public Guid RegisterClient(string connectionId, string nickName, IClientProxy client)
        {
            if (gameEngine == default ||
                gameLogger == default)
            {
                return default;
            }

            if (TotalConnectedClients >= runnerConfig.BotCount)
            {
                return default;
            }

            var botGuid = Guid.NewGuid();
            Logger.LogDebug("RunnerStateService", "Registering Bot");
            gameEngine.Client.SendAsync("BotRegistered", botGuid);
            TryAdd(ActiveConnections, botGuid, connectionId);
            TryAdd(AllConnections, botGuid, connectionId);

            if (nickName == null)
            {
                return botGuid;
            }

            nickName = nickName.Length <= 12 ? nickName : nickName.Substring(0, 12);
            TryAdd(NickNames, botGuid, nickName);

            return botGuid;
        }

        public void DeregisterBot(string connectionId)
        {
            KeyValuePair<Guid, string> connection = ActiveConnections.FirstOrDefault(c => c.Value == connectionId);

            if (connection.Key == default)
            {
                return;
            }

            Logger.LogDebug("Remove Connection", $"Removing Connection ID: {connection.Value} for Bot Guid: {connection.Key.ToString()}");
            var removalResult = ActiveConnections.TryRemove(connection.Key, out _);
            while (!removalResult)
            {
                removalResult = ActiveConnections.TryRemove(connection.Key, out _);
            }
        }

        public Guid? GetBotGuidFromConnectionId(string connectionId)
        {
            var (key, value) = ActiveConnections.FirstOrDefault(c => c.Value == connectionId);

            if (key == default)
            {
                return null;
            }

            return key;
        }

        public void StartGame()
        {
            Logger.LogInfo("RunnerState", "Starting Game");
            gameEngine.Client.SendAsync("StartGame");
        }

        public void RegisterEngine(string connectionId, IClientProxy client)
        {
            Logger.LogInfo("RunnerState", "Registering Engine");
            gameEngine = new StateObject
            {
                ConnectionId = connectionId,
                Client = client
            };
        }

        public void RegisterLogger(string connectionId, IClientProxy client)
        {
            Logger.LogInfo("RunnerState", "Registering Logger");
            gameLogger = new StateObject
            {
                ConnectionId = connectionId,
                Client = client
            };
        }

        public void AddRegistrationToken(string connectionId, Guid token)
        {
            TryAdd(RegistrationTokens, token, connectionId);
        }

        private void TryAdd(ConcurrentDictionary<Guid,string> dict, Guid token, string entry)
        {
            var result = false;
            do
            {
                result = dict.TryAdd(token, entry);
            } while (!result);
        }

        public string GetRegistrationToken(string botId)
        {
            var botGuid = Guid.Parse(botId);

            if (!AllConnections.TryGetValue(botGuid, out var botConnectionId))
            {
                return null;
            }

            var (key, value) = RegistrationTokens.FirstOrDefault(c => c.Value == botConnectionId);

            return key == default ? null : key.ToString();
        }

        public void StopApplication()
        {
            applicationLifetime.StopApplication();
        }

        public bool GetBotActionReceived(Guid id)
        {
            return BotActionsReceived.Exists(b => b == id);
        }

        public void AddBotActionReceived(Guid id)
        {
            BotActionsReceived.Add(id);
        }

        public void ClearBotActionsReceived()
        {
            BotActionsReceived = new List<Guid>();
        }
    }
}
