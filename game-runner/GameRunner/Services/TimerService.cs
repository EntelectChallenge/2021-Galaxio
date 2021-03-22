using System.Collections.Generic;
using System.Timers;
using Domain.Services;
using GameRunner.Enums;
using GameRunner.Interfaces;
using GameRunner.Models;

namespace GameRunner.Services
{
    public class TimerService : ITimerService
    {
        private Timer componentTimer;
        private Timer botTimer;
        private readonly IRunnerStateService runnerStateService;
        private readonly RunnerConfig runnerConfig;
        private readonly ICloudIntegrationService cloudIntegrationService;

        public TimerService(
            IRunnerStateService runnerStateService,
            IConfigurationService configurationService,
            ICloudIntegrationService cloudIntegrationService)
        {
            this.runnerStateService = runnerStateService;
            this.cloudIntegrationService = cloudIntegrationService;
            this.runnerConfig = configurationService.RunnerConfig;
        }

        public void StartTimeoutEvents()
        {
            CheckConnectedBots();
            CheckGameComponents();
        }

        private void CheckConnectedBots()
        {
            botTimer = new Timer();
            botTimer.Interval = runnerConfig.BotTimeoutInMs;
            botTimer.Elapsed += BotConnectionTimeout;
            botTimer.AutoReset = false;
            botTimer.Enabled = true;
        }

        private void BotConnectionTimeout(object sender, ElapsedEventArgs e)
        {
            if (runnerStateService.TotalConnections < runnerConfig.BotCount)
            {
                Logger.LogDebug(
                    "RunnerHub.OnBotConnectionTimeout",
                    string.Format(
                        "{0} out of {1} bots connected in time, runner is shutting down.",
                        runnerStateService.TotalConnections,
                        runnerConfig.BotCount));
                cloudIntegrationService.Announce(CloudCallbackType.Failed)
                    .GetAwaiter()
                    .OnCompleted(() => runnerStateService.StopApplication());
            }
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

            if (runnerStateService.GetEngine() == default)
            {
                components.Add("GameEngine");
                componentTimedOut = true;
            }

            if (runnerStateService.GetLogger() == default)
            {
                components.Add("Logger");
                componentTimedOut = true;
            }

            if (componentTimedOut)
            {
                Logger.LogDebug(
                    "RunnerHub.OnComponentTimeout",
                    string.Format("The following components did not connect before timeout: {0}", string.Join(", ", components.ToArray())));
                cloudIntegrationService.Announce(CloudCallbackType.Failed)
                    .GetAwaiter()
                    .OnCompleted(() => runnerStateService.StopApplication());
            }
        }
    }
}