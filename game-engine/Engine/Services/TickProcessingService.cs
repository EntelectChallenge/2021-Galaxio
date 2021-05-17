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

        public void SimulateTick()
        {
            if (worldStateService.GetPlayerCount() <= 1)
            {
                return;
            }

            var movingObjects = worldStateService.GetMovableObjects();

            var consumedItems = new List<MovableGameObject>();
            var simulationStep = 0;
            /*
             * Determine the endpoint of the travel path of the bot
             * Collect their collision points along that path
             */
            List<MovementPath> movementPaths = (from movableGameObject in movingObjects
                where movableGameObject.IsMoving
                let endpoint = vectorCalculatorService.GetPointFrom(movableGameObject.Position, movableGameObject.Speed, movableGameObject.CurrentHeading)
                select new MovementPath
                {
                    Mover = movableGameObject,
                    MovementEndpoint = endpoint,
                    MovementStartPoint = movableGameObject.Position,
                    CollisionDetectionPoints =
                        vectorCalculatorService.CollectCollisionDetectionPointsAlongPath(movableGameObject.Position, endpoint, movableGameObject.CurrentHeading)
                }).ToList();

            while (movementPaths.Any())
            {
                /*
                 * Move all bots to their next collision detection point this cycle
                 */
                ApplyNextCollisionPoint(movementPaths, simulationStep);

                /*
                 * Compute collisions for all bots and apply their effects
                 */
                ApplyCollisionsAtCollisionPoint(movementPaths, consumedItems);

                /*
                 * Remove all consumed bots, and any points that have completed their travel.
                 * Continue until all bots are either consumed in this tick, or they have fully travelled their path
                 */
                movementPaths = movementPaths.Where(
                        movementPath => !consumedItems.Contains(movementPath.Mover) && movementPath.CollisionDetectionPoints.Any())
                    .ToList();
                simulationStep++;
            }
        }

        private void ApplyNextCollisionPoint(List<MovementPath> movememntPaths, int simulationStep)
        {
            foreach (var movementPath in movememntPaths)
            {
                ValidateOutstandingPath(movementPath, simulationStep);
                var possiblePosition = movementPath.CollisionDetectionPoints.FirstOrDefault();
                if (possiblePosition != null)
                {
                    movementPath.Moved = true;
                    movementPath.Mover.Position = possiblePosition;
                    movementPath.CollisionDetectionPoints.RemoveAt(0);
                }
                else
                {
                    movementPath.Moved = false;
                }
            }
        }

        private int ValidateOutstandingPath(MovementPath movementPath, int simulationStep)
        {
            var invalidCount = 0;
            var distanceToTravel = movementPath.Mover.Speed;

            if (movementPath.HasCollided)
            {
                distanceToTravel -= simulationStep;
                if (distanceToTravel < 0)
                {
                    distanceToTravel = 0;
                }

                var endpoint = vectorCalculatorService.GetPointFrom(movementPath.Mover.Position, distanceToTravel, movementPath.Mover.CurrentHeading);

                movementPath.CollisionDetectionPoints = vectorCalculatorService.CollectCollisionDetectionPointsAlongPath(
                    movementPath.Mover.Position,
                    endpoint,
                    movementPath.Mover.CurrentHeading);

                return invalidCount;
            }

            var finalPointIsValid = false;
            var distanceFromStart = vectorCalculatorService.GetDistanceBetween(movementPath.MovementStartPoint, movementPath.Mover.Position);

            while (!finalPointIsValid)
            {
                if (!movementPath.CollisionDetectionPoints.Any())
                {
                    finalPointIsValid = true;
                    break;
                }
                var distanceToEndpoint = vectorCalculatorService.GetDistanceBetween(
                    movementPath.Mover.Position,
                    movementPath.CollisionDetectionPoints.Last());
                if (distanceFromStart + distanceToEndpoint > distanceToTravel)
                {
                    invalidCount++;
                    movementPath.CollisionDetectionPoints.RemoveAt(movementPath.CollisionDetectionPoints.Count - 1);
                }
                else
                {
                    finalPointIsValid = true;
                }
            }

            return invalidCount;
        }

        private void ApplyCollisionsAtCollisionPoint(List<MovementPath> movementPaths, List<MovableGameObject> consumedBots)
        {
            foreach (var movementPath in movementPaths)
            {
                if (!movementPath.Moved)
                {
                    continue;
                }

                var bot = movementPath.Mover;
                List<GameObject> collisions = collisionService.GetCollisions(bot);
                var botIsAlive = collisions.Select(
                        gameObject =>
                        {
                            movementPath.HasCollided = true;
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