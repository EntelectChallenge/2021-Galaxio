using System;
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

        private readonly Dictionary<Guid, string> ActiveConnections = new Dictionary<Guid, string>();
        private readonly Dictionary<Guid, string> AllConnections = new Dictionary<Guid, string>();
        private readonly Dictionary<Guid, string> RegistrationTokens = new Dictionary<Guid, string>();
        private readonly Dictionary<Guid, string> NickNames = new Dictionary<Guid, string>();
        public List<Guid> BotActionsReceived = new List<Guid>();

        private GameCompletePayload gameCompletePayload;
        private readonly IHostApplicationLifetime applicationLifetime;

        public int TotalConnectedClients => ActiveConnections.Count;
        public int TotalConnectedBots => RegistrationTokens.Count;
        public int TotalConnections => AllConnections.Count;
        public string FailureReason { get; }
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

            CheckGameComponents();
        }

        public StateObject GetEngine() => gameEngine;

        public StateObject GetLogger() => gameLogger;
        public Dictionary<Guid, string> GetActiveConnections() => ActiveConnections;

        public Dictionary<Guid, string> GetRegistrationTokens() => RegistrationTokens;

        public Guid RegisterClient(string connectionId, string nickName, IClientProxy client)
        {
            if (gameEngine == default ||
                gameLogger == default)
            {
                return default;
            }

            var botGuid = Guid.NewGuid();
            Logger.LogDebug("RunnerStateService", "Registering Bot");
            gameEngine.Client.SendAsync("BotRegistered", botGuid);
            ActiveConnections.Add(botGuid, connectionId);
            AllConnections.Add(botGuid, connectionId);

            if (nickName == null)
            {
                return botGuid;
            }

            nickName = nickName.Length <= 12 ? nickName : nickName.Substring(0, 12);
            NickNames.Add(botGuid, nickName);

            return botGuid;
        }

        public void DeregisterBot(string connectionId)
        {
            KeyValuePair<Guid, string> connection = ActiveConnections.FirstOrDefault(c => c.Value == connectionId);

            if (connection.Key == default)
            {
                return;
            }

            Logger.LogDebug("Remove Connection", $"Removing Connection ID: {connection.Key} for Bot Guid: {connection.Value.ToString()}");
            Logger.LogDebug("ActiveConnections", $"Active Connections Entry Count: {ActiveConnections.Count}");
            ActiveConnections.Remove(connection.Key);
            Logger.LogDebug("ActiveConnections", $"Active Connections Post Removal Entry Count: {ActiveConnections.Count}");
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
            RegistrationTokens.Add(token, connectionId);
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

        private void CheckGameComponents()
        {
            componentTimer = new Timer();
            componentTimer.Interval = runnerConfig.ComponentTimeoutInMs;
            componentTimer.Elapsed += ComponentsConnectionTimeout;
            componentTimer.AutoReset = false;
            componentTimer.Enabled = true;
        }

        private void ComponentsConnectionTimeout(object sender, ElapsedEventArgs e)
        {
            var componentTimedOut = false;
            var components = new List<string>();

            if (GetEngine() == default)
            {
                components.Add("GameEngine");
                componentTimedOut = true;
            }

            if (GetLogger() == default)
            {
                components.Add("Logger");
                componentTimedOut = true;
            }

            if (componentTimedOut)
            {
                Logger.LogDebug(
                    "RunnerHub.OnComponentTimeout",
                    string.Format("The following components did not connect before timeout: {0}", string.Join(", ", components.ToArray())));
                StopApplication();
            }
        }
    }
}