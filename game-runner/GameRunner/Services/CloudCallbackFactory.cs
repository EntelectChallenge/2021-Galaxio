using System;
using System.Collections.Generic;
using System.Linq;
using GameRunner.Enums;
using GameRunner.Interfaces;
using GameRunner.Models;
using Newtonsoft.Json;

namespace GameRunner.Services
{
    public class CloudCallbackFactory : ICloudCallbackFactory
    {
        private readonly IEnvironmentService environmentService;
        private readonly RunnerConfig runnerConfig;
        private readonly IRunnerStateService runnerStateService;

        public CloudCallbackFactory(
            IEnvironmentService environmentService,
            IConfigurationService runnerConfig,
            IRunnerStateService runnerStateService)
        {
            this.environmentService = environmentService;
            this.runnerStateService = runnerStateService;
            this.runnerConfig = runnerConfig.RunnerConfig;
        }

        public CloudCallback Make(CloudCallbackType callbackType)
        {
            return callbackType switch
            {
                CloudCallbackType.Initializing => new CloudCallback
                {
                    MatchId = environmentService.MatchId,
                    MatchStatus = "initializing",
                    MatchStatusReason = "Startup"
                },
                CloudCallbackType.Ready => new CloudCallback
                {
                    MatchId = environmentService.MatchId,
                    MatchStatus = "ready",
                    MatchStatusReason = $"All Components connected and ready for bots. Waiting for {runnerConfig.BotCount} bots to connect."
                },
                CloudCallbackType.Started => new CloudCallback
                {
                    MatchId = environmentService.MatchId,
                    MatchStatus = "started",
                    MatchStatusReason = $"Match has started with {runnerStateService.TotalConnectedBots} bots"
                },
                CloudCallbackType.Failed => new CloudCallback
                {
                    MatchId = environmentService.MatchId,
                    MatchStatus = "failed",
                    MatchStatusReason = runnerStateService.FailureReason,
                    Players = MakeFailedPlayerList()
                },
                CloudCallbackType.Finished => new CloudCallback
                {
                    MatchId = environmentService.MatchId,
                    MatchStatus = "finished",
                    MatchStatusReason = "Game Complete.",
                    Seed = JsonConvert.SerializeObject(runnerStateService.GameCompletePayload.WorldSeeds),
                    Ticks = runnerStateService.GameCompletePayload.TotalTicks.ToString(),
                    Players = MakePlayerList()
                },
                CloudCallbackType.LoggingComplete => new CloudCallback
                {
                    MatchId = environmentService.MatchId,
                    MatchStatus = "logging_complete",
                    MatchStatusReason = "Game Complete. Logging Complete.",
                    Seed = JsonConvert.SerializeObject(runnerStateService.GameCompletePayload.WorldSeeds),
                    Ticks = runnerStateService.GameCompletePayload.TotalTicks.ToString(),
                    Players = MakePlayerList()
                },
                _ => throw new ArgumentOutOfRangeException(nameof(callbackType), callbackType, "Unknown Cloud Callback Type")
            };
        }

        private List<CloudPlayer> MakeFailedPlayerList() =>
            runnerStateService.GetRegistrationTokens()
                .Select(
                    playerToken => new CloudPlayer
                    {
                        PlayerParticipantId = playerToken.Key.ToString()
                    })
                .ToList();

        private List<CloudPlayer> MakePlayerList() =>
            runnerStateService.GameCompletePayload.Players.Select(
                    playerResult => new CloudPlayer
                    {
                        Placement = playerResult.Placement,
                        Seed = playerResult.Seed,
                        FinalScore = playerResult.Score,
                        GamePlayerId = playerResult.Id,
                        PlayerParticipantId = runnerStateService.GetRegistrationToken(playerResult.Id),
                        MatchPoints = playerResult.MatchPoints
                    })
                .ToList();
    }
}