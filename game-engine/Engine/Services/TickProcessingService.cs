using System.Collections.Generic;
using System.Linq;
using Domain.Models;
using Engine.Handlers.Interfaces;
using Engine.Interfaces;

namespace Engine.Services
{
    public class TickProcessingService : ITickProcessingService
    {
        private readonly IVectorCalculatorService vectorCalculatorService;
        private readonly IWorldStateService worldStateService;
        private readonly ICollisionHandlerResolver collisionHandlerResolver;
        private readonly ICollisionService collisionService;

        public TickProcessingService(
            ICollisionHandlerResolver collisionHandlerResolver,
            IVectorCalculatorService vectorCalculatorService,
            IWorldStateService worldStateService,
            ICollisionService collisionService)
        {
            this.collisionHandlerResolver = collisionHandlerResolver;
            this.vectorCalculatorService = vectorCalculatorService;
            this.worldStateService = worldStateService;
            this.collisionService = collisionService;
        }

        public void SimulateTick(BotObject bot)
        {
            if (!bot.IsMoving)
            {
                return;
            }

            for (var i = 0; i < bot.Speed; i++)
            {
                if (worldStateService.GetPlayerCount() <= 1)
                {
                    break;
                }

                bot.Position = vectorCalculatorService.MovePlayerObject(bot.Position, 1, bot.CurrentHeading);

                List<GameObject> collisions = collisionService.GetCollisions(bot);
                var botIsAlive = collisions.Select(
                        gameObject =>
                        {
                            var handler = collisionHandlerResolver.ResolveHandler(gameObject, bot);
                            return handler.ResolveCollision(gameObject, bot);
                        })
                    .All(alive => alive);

                if (botIsAlive)
                {
                    continue;
                }

                worldStateService.RemoveGameObjectById(bot.Id);
                break;
            }
        }
    }
}