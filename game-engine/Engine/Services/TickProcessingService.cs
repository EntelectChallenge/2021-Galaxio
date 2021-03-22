using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.Models;
using Engine.Handlers.Interfaces;
using Engine.Interfaces;
using Engine.Models;

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

        public void SimulateTick(IList<BotObject> bots)
        {
            if (worldStateService.GetPlayerCount() <= 1)
            {
                return;
            }

            var consumedBots = new List<BotObject>();
            var simulationStep = 0;
            /*
             * Determine the endpoint of the travel path of the bot
             * Collect their collision points along that path
             */
            List<BotPath> botPaths = (from bot in bots
                where bot.IsMoving
                let endpoint = vectorCalculatorService.GetPointFrom(bot.Position, bot.Speed, bot.CurrentHeading)
                select new BotPath
                {
                    Bot = bot,
                    MovementEndpoint = endpoint,
                    MovementStartPoint = bot.Position,
                    CollisionDetectionPoints =
                        vectorCalculatorService.CollectCollisionDetectionPointsAlongPath(bot.Position, endpoint, bot.CurrentHeading)
                }).ToList();

            while (botPaths.Any())
            {
                /*
                 * Move all bots to their next collision detection point this cycle
                 */
                ApplyNextCollisionPoint(botPaths, simulationStep);

                /*
                 * Compute collisions for all bots and apply their effects
                 */
                ApplyCollisionsAtCollisionPoint(botPaths, consumedBots);

                /*
                 * Remove all consumed bots, and any points that have completed their travel.
                 * Continue until all bots are either consumed in this tick, or they have fully travelled their path
                 */
                botPaths = botPaths.Where(
                        botMovement => !consumedBots.Contains(botMovement.Bot) && botMovement.CollisionDetectionPoints.Any())
                    .ToList();
                simulationStep++;
            }
        }

        private void ApplyNextCollisionPoint(List<BotPath> botPaths, int simulationStep)
        {
            foreach (var botPath in botPaths)
            {
                ValidateOutstandingPath(botPath, simulationStep);
                var possiblePosition = botPath.CollisionDetectionPoints.FirstOrDefault();
                if (possiblePosition != null)
                {
                    botPath.Moved = true;
                    botPath.Bot.Position = possiblePosition;
                    botPath.CollisionDetectionPoints.RemoveAt(0);
                }
                else
                {
                    botPath.Moved = false;
                }
            }
        }

        private int ValidateOutstandingPath(BotPath botPath, int simulationStep)
        {
            var invalidCount = 0;
            var distanceToTravel = botPath.Bot.Speed;

            if (botPath.HasCollided)
            {
                distanceToTravel -= simulationStep;
                if (distanceToTravel < 0)
                {
                    distanceToTravel = 0;
                }

                var endpoint = vectorCalculatorService.GetPointFrom(botPath.Bot.Position, distanceToTravel, botPath.Bot.CurrentHeading);

                botPath.CollisionDetectionPoints = vectorCalculatorService.CollectCollisionDetectionPointsAlongPath(
                    botPath.Bot.Position,
                    endpoint,
                    botPath.Bot.CurrentHeading);

                return invalidCount;
            }

            var finalPointIsValid = false;
            var distanceFromStart = vectorCalculatorService.GetDistanceBetween(botPath.MovementStartPoint, botPath.Bot.Position);

            while (!finalPointIsValid)
            {
                if (!botPath.CollisionDetectionPoints.Any())
                {
                    finalPointIsValid = true;
                    break;
                }
                var distanceToEndpoint = vectorCalculatorService.GetDistanceBetween(
                    botPath.Bot.Position,
                    botPath.CollisionDetectionPoints.Last());
                if (distanceFromStart + distanceToEndpoint > distanceToTravel)
                {
                    invalidCount++;
                    botPath.CollisionDetectionPoints.RemoveAt(botPath.CollisionDetectionPoints.Count - 1);
                }
                else
                {
                    finalPointIsValid = true;
                }
            }

            return invalidCount;
        }

        private void ApplyCollisionsAtCollisionPoint(List<BotPath> botsWithMovementPoints, List<BotObject> consumedBots)
        {
            foreach (var botLine in botsWithMovementPoints)
            {
                if (!botLine.Moved)
                {
                    continue;
                }

                var bot = botLine.Bot;
                List<GameObject> collisions = collisionService.GetCollisions(bot);
                var botIsAlive = collisions.Select(
                        gameObject =>
                        {
                            botLine.HasCollided = true;
                            var handler = collisionHandlerResolver.ResolveHandler(gameObject, bot);
                            return handler.ResolveCollision(gameObject, bot);
                        })
                    .All(alive => alive);

                if (botIsAlive)
                {
                    continue;
                }

                worldStateService.RemoveGameObjectById(bot.Id);
                consumedBots.Add(bot);
            }
        }
    }
}