using System.Collections.Generic;
using Domain.Enums;

namespace Engine.Models
{
    public class EngineConfig
    {
        public string RunnerUrl { get; set; }
        public string RunnerPort { get; set; }
        public int BotCount { get; set; }
        public int MaxRounds { get; set; }
        public int TickRate { get; set; }
        public int MapRadius { get; set; }
        public int MapRadiusRatio { get; set; }
        public int StartRadius { get; set; }
        public int StartRadiusRatio { get; set; }
        public int StartingPlayerSize { get; set; }
        public Dictionary<GameObjectType, decimal> ConsumptionRatio { get; set; }
        public Dictionary<GameObjectType, int> ScoreRates { get; set; }

        public SpeedConfig Speeds { get; set; }

        public Seeds Seeds { get; set; }
        public WorldFood WorldFood { get; set; }
        public int MinimumPlayerSize { get; set; }
        public WormholeConfig Wormholes { get; set; }
        public AfterburnerConfig Afterburners { get; set; }
        public WorldObstacleConfig GasClouds { get; set; }
        public WorldObstacleConfig AsteroidFields { get; set; }
    }

    public class WormholeConfig
    {
        public int Count { get; set; }
        public int CountRatio { get; set; }
        public int? Seed { get; set; }
        public int MinSeed { get; set; }
        public int MaxSeed { get; set; }
        public int StartSize { get; set; }
        public int MinSize { get; set; }
        public int MaxSize { get; set; }
        public int MinSeparation { get; set; }
        public decimal GrowthRate { get; set; }
    }

    public class SpeedConfig
    {
        public int Minimum { get; set; }
        public int Maximum { get; set; }
        public decimal Ratio { get; set; }
        public int StartingSpeed { get; set; }
    }

    public class Seeds
    {
        public List<int> PlayerSeeds { get; set; }
        public int MaxSeed { get; set; }
        public int MinSeed { get; set; }
    }

    public class WorldFood
    {
        public int MaxStartingSeparation { get; set; }
        public int MaxSeparation { get; set; }
        public int PlayerSafeFood { get; set; }
        public int StartingFoodCount { get; set; }
        public int StartingFoodCountRatio { get; set; }
        public int FoodSize { get; set; }
        public int MinSeparation { get; set; }
        public int MaxConsumptionSize { get; set; }
    }

    public class AfterburnerConfig
    {
        public int SizeConsumptionPerTick { get; set; }
        public int SpeedFactor { get; set; }
    }

    public class WorldObstacleConfig
    {
        public int GenerateCount { get; set; }
        public int MaxCount { get; set; }
        public decimal MaxCountRatio { get; set; }
        public int? Seed { get; set; }
        public int MinSeed { get; set; }
        public int MaxSeed { get; set; }
        public int Modular { get; set; }
        public int ModularRatio { get; set; }
        public int GenerateSubCount { get; set; }
        public int MaxSubCount { get; set; }
        public int SubModular { get; set; }
        public int Multiplier { get; set; }
        public decimal ConstX { get; set; }
        public decimal ConstY { get; set; }
        public int ConstXY { get; set; }
        public int RepeatingValuesLimit { get; set; }
        public int MinDistanceFromPlayers { get; set; }
        public decimal NodeSizeMultiplier { get; set; }
        public int AffectPerTick { get; set; }
    }
}