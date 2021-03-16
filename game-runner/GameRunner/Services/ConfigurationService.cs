using System;
using GameRunner.Extensions;
using GameRunner.Interfaces;
using GameRunner.Models;
using Microsoft.Extensions.Options;

namespace GameRunner.Services
{
    public class ConfigurationService : IConfigurationService
    {
        public RunnerConfig RunnerConfig { get; set; }

        public ConfigurationService(IOptions<RunnerConfig> config)
        {
            RunnerConfig = new RunnerConfig();
            config.Value.CopyPropertiesTo(RunnerConfig);

            if (int.TryParse(Environment.GetEnvironmentVariable("BOT_COUNT"), out var botCount))
            {
                RunnerConfig.BotCount = botCount;
            }
            if (int.TryParse(Environment.GetEnvironmentVariable("COMPONENT_TIMEOUT"), out var componentTimeout))
            {
                RunnerConfig.ComponentTimeoutInMs = componentTimeout;
            }
            if (int.TryParse(Environment.GetEnvironmentVariable("BOT_TIMEOUT"), out var botTimeout))
            {
                RunnerConfig.BotTimeoutInMs = botTimeout;
            }

        }
    }
}