using System;
using Domain.Services;
using Engine.Extensions;
using Engine.Models;
using Microsoft.Extensions.Options;

namespace Engine.Services
{
    public class ConfigurationService: IConfigurationService
    {
        public EngineConfig Value { get; set; }

        public ConfigurationService(IOptions<EngineConfig> engineOptions)
        {
            this.Value = new EngineConfig();
            engineOptions.Value.CopyPropertiesTo(Value);

            var botCountEnvarString = Environment.GetEnvironmentVariable("BOT_COUNT");
            if (string.IsNullOrWhiteSpace(botCountEnvarString))
            {
                return;
            }

            var botCount = int.Parse(botCountEnvarString);
            Value.BotCount = botCount;
            Value.MapRadius = botCount * Value.MapRadiusRatio;
            Value.MaxRounds = Value.MapRadius;
            Value.StartRadius = botCount * Value.StartRadiusRatio;
            Value.WorldFood.StartingFoodCount = botCount * Value.WorldFood.StartingFoodCountRatio;
            Value.Wormholes.Count = botCount * Value.Wormholes.CountRatio;
            Value.GasClouds.MaxCount = (int)Math.Ceiling(botCount * Value.GasClouds.MaxCountRatio);
            Value.GasClouds.Modular = GetOddModularValue(Value.GasClouds.ModularRatio, botCount);
            Value.AsteroidFields.MaxCount = (int)Math.Ceiling(botCount * Value.AsteroidFields.MaxCountRatio);
            Value.AsteroidFields.Modular = GetOddModularValue(Value.AsteroidFields.ModularRatio, botCount);
        }

        private int GetOddModularValue(int ratio, int botCount)
        {
            return (ratio * botCount) % 2 == 0 ? (ratio * botCount) + 1 : (ratio * botCount);
        }
    }

    public interface IConfigurationService
    {
        public EngineConfig Value { get; set; }
    }
}