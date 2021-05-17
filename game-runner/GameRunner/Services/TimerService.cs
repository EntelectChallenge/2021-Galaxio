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
                var failReason = $"{runnerStateService.TotalConnections} out of {runnerConfig.BotCount} bots connected in time, runner is shutting down.";
                Logger.LogDebug(
                    "RunnerHub.OnBotConnectionTimeout",
                    failReason);

                runnerStateService.FailureReason = failReason;
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
                var failReason = $"The following components did not connect before timeout: {string.Join(", ", components.ToArray())}";
                Logger.LogDebug(
                    "RunnerHub.OnComponentTimeout",
                    failReason);
                runnerStateService.FailureReason = failReason;
                cloudIntegrationService.Announce(CloudCallbackType.Failed)
                    .GetAwaiter()
                    .OnCompleted(() => runnerStateService.StopApplication());
            }
        }
    }
}