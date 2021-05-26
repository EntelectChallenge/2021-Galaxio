using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Models;
using Engine.Interfaces;
using Engine.Models;

namespace Engine.Services
{
    public class CollisionService : ICollisionService
    {
        private readonly EngineConfig engineConfig;
        private readonly IWorldStateService worldStateService;
        private readonly IVectorCalculatorService vectorCalculatorService;

        public CollisionService(
            IConfigurationService engineConfig,
            IWorldStateService worldStateService,
            IVectorCalculatorService vectorCalculatorService)
        {
            this.worldStateService = worldStateService;
            this.vectorCalculatorService = vectorCalculatorService;
            this.engineConfig = engineConfig.Value;
        }

        public int GetConsumedSizeFromPlayer(GameObject consumer, GameObject consumee) =>
            (int) Math.Ceiling(Math.Max(consumer.Size * engineConfig.ConsumptionRatio[consumer.GameObjectType], consumee.Size));

        public List<GameObject> GetCollisions(MovableGameObject bot)
        {
            IList<GameObject> gameObjects = worldStateService.GetCurrentGameObjects();
            return gameObjects.Where(go => go.Id != bot.Id && vectorCalculatorService.HasOverlap(go, bot)).ToList();
        }
    }
}