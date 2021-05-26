using System;
using Domain.Enums;
using Domain.Models;
using Engine.Handlers.Interfaces;
using Engine.Interfaces;
using Engine.Models;
using Engine.Services;

namespace Engine.Handlers.Collisions
{
    public class WormholeCollisionHandler : ICollisionHandler
    {
        private readonly EngineConfig engineConfig;
        private readonly IWorldStateService worldStateService;
        private readonly IVectorCalculatorService vectorCalculatorService;

        public WormholeCollisionHandler(
            IWorldStateService worldStateService,
            IVectorCalculatorService vectorCalculatorService,
            IConfigurationService engineConfigOptions)
        {
            this.worldStateService = worldStateService;
            this.vectorCalculatorService = vectorCalculatorService;
            engineConfig = engineConfigOptions.Value;
        }

        public bool IsApplicable(GameObject gameObject, MovableGameObject mover) => gameObject.GameObjectType == GameObjectType.Wormhole;

        public bool ResolveCollision(GameObject gameObject, MovableGameObject mover)
        {
            if (gameObject.Size < mover.Size)
            {
                return true;
            }

            Tuple<GameObject, GameObject> wormholePair = worldStateService.GetWormholePair(gameObject.Id);
            if (wormholePair.Equals(null))
            {
                // Lol how? We found a wormhole in state that did not exist in the known wormhole pairs
                throw new InvalidOperationException("Invalid Wormhole");
            }

            var counterpartWormhole = wormholePair.Item1.Id == gameObject.Id ? wormholePair.Item2 : wormholePair.Item1;

            var resultingPosition = vectorCalculatorService.GetPositionFrom(
                counterpartWormhole.Position,
                counterpartWormhole.Size + mover.Size,
                mover.CurrentHeading);

            mover.Position = resultingPosition;
            if (mover is BotObject botObject)
            {
                botObject.Score = engineConfig.ScoreRates[GameObjectType.Wormhole];
            }

            var newSize = (int) Math.Ceiling(wormholePair.Item1.Size * engineConfig.ConsumptionRatio[GameObjectType.Wormhole]);
            wormholePair.Item1.Size = newSize < engineConfig.Wormholes.MinSize ? engineConfig.Wormholes.MinSize : newSize;
            wormholePair.Item2.Size = wormholePair.Item1.Size;

            worldStateService.UpdateGameObject(wormholePair.Item1);
            worldStateService.UpdateGameObject(wormholePair.Item2);

            return true;
        }
    }
}