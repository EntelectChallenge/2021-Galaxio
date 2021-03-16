using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Domain.Enums;
using Domain.Models;
using Engine.Interfaces;
using Engine.Models;
using Microsoft.Extensions.Options;

namespace Engine.Services
{
    public class WorldObjectGenerationService : IWorldObjectGenerationService
    {
        private readonly EngineConfig engineConfig;
        private readonly IVectorCalculatorService vectorCalculatorService;

        public WorldObjectGenerationService(IConfigurationService engineConfigOptions, IVectorCalculatorService vectorCalculatorService)
        {
            engineConfig = engineConfigOptions.Value;
            this.vectorCalculatorService = vectorCalculatorService;
        }

        public List<GameObject> GenerateWorldFood(List<GameObject> placedFood, List<int> playerSeeds, List<GameObject> gameObjects)
        {
            // Food must always be divisible by number of bots
            var foodPerPlacedFood = (engineConfig.WorldFood.StartingFoodCount - placedFood.Count) / placedFood.Count;
            var startFoodTraces = placedFood.Count;
            for (var index = 0; index < startFoodTraces; index++)
            {
                var placedFoodTracker = foodPerPlacedFood;
                var playerIndex = index % engineConfig.BotCount;
                var playerSeed = playerSeeds[playerIndex];
                var originFood = placedFood[index];

                var lastPlacedFood = FirstFoodPlacementCalculation(originFood, playerSeed, gameObjects, placedFood);
                playerSeed += playerSeed % 359;

                for (var i = 1; i < placedFoodTracker; i++)
                {
                    var heading = GetHeadingFromSeed(playerSeed);
                    var distance = GetDistanceFromSeed(
                        playerSeed,
                        engineConfig.WorldFood.FoodSize * 2 + engineConfig.WorldFood.MinSeparation,
                        engineConfig.WorldFood.MaxSeparation);
                    var changeVariable = playerSeed % 359;
                    playerSeed += changeVariable;

                    //To stop infinite loop if mod is 0
                    if (changeVariable == 0)
                    {
                        playerSeed += 1;
                    }

                    var foodPosition = vectorCalculatorService.GetPositionFrom(lastPlacedFood.Position, distance, heading);
                    var food = CreateFoodObjectAtPosition(Guid.NewGuid(), foodPosition);
                    var isValid = CheckPlacementValidity(
                        food,
                        placedFood,
                        lastPlacedFood.Position,
                        engineConfig.WorldFood.FoodSize * 2 + engineConfig.WorldFood.MinSeparation,
                        engineConfig.WorldFood.MaxSeparation);
                    if (!isValid)
                    {
                        placedFoodTracker++;
                    }
                    else
                    {
                        gameObjects.Add(food);
                        placedFood.Add(food);
                        lastPlacedFood = food;
                    }
                }
            }

            return gameObjects;
        }

        public List<Tuple<GameObject, GameObject>> GenerateWormholes(List<GameObject> gameObjects, int wormholeSeed)
        {
            var totalWormholesLeftToPlace = engineConfig.Wormholes.Count;
            var originWormholeCount = totalWormholesLeftToPlace / 2;
            var worldCenter = new Position();

            var wormholePairs = new List<Tuple<GameObject, GameObject>>();

            for (var i = 0; i < originWormholeCount; i++)
            {
                var placedWormhole = PlaceWormholeWithOrigin(
                    gameObjects,
                    ref wormholeSeed,
                    worldCenter,
                    engineConfig.MapRadius,
                    engineConfig.Wormholes.MinSeparation,
                    true);
                var counterpartWormhole = PlaceWormholeWithOrigin(
                    gameObjects,
                    ref wormholeSeed,
                    placedWormhole.Position,
                    engineConfig.MapRadius * 2,
                    engineConfig.Wormholes.MinSeparation,
                    false);
                var pair = new Tuple<GameObject, GameObject>(placedWormhole, counterpartWormhole);

                wormholePairs.Add(pair);
            }

            return wormholePairs;
        }

        public GameObject PlaceWormholeWithOrigin(
            List<GameObject> gameObjects,
            ref int wormholeSeed,
            Position worldCenter,
            int maxPlacementDistance,
            int minPlacementDistance,
            bool isFirstWormhole)
        {
            var isValid = false;
            var wormhole = new GameObject();
            while (!isValid)
            {
                var heading = GetHeadingFromSeed(wormholeSeed);
                var distance = GetDistanceFromSeed(wormholeSeed, minPlacementDistance, maxPlacementDistance);

                wormholeSeed += heading;

                //To stop infinite loop if mod is 0
                if (heading == 0)
                {
                    wormholeSeed += 1;
                }

                var wormholePosition = vectorCalculatorService.GetPositionFrom(worldCenter, distance, heading);
                wormhole = CreateObjectAtPosition(
                    Guid.NewGuid(),
                    wormholePosition,
                    GameObjectType.Wormhole,
                    engineConfig.Wormholes.StartSize);
                isValid = CheckPlacementValidity(
                    wormhole,
                    gameObjects.Where(go => go.GameObjectType == GameObjectType.Wormhole).ToList(),
                    worldCenter,
                    engineConfig.Wormholes.MaxSize * 2 + engineConfig.Wormholes.MinSeparation,
                    engineConfig.MapRadius * 2,
                    isFirstWormhole);
            }

            gameObjects.Add(wormhole);

            return wormhole;
        }

        public List<GameObject> GeneratePlayerStartingFood(List<int> playerSeeds, List<GameObject> gameObjects)
        {
            var placedFood = new List<GameObject>();
            for (var i = 0; i < playerSeeds.Count; i++)
            {
                var playerSeed = playerSeeds[i];
                var totalPlayers = engineConfig.BotCount;

                var startingPositions = new List<Position>();

                for (var playerNumber = 0; playerNumber < totalPlayers; playerNumber++)
                {
                    startingPositions.Add(
                        vectorCalculatorService.GetNewPlayerStartingPosition(
                            playerNumber,
                            engineConfig.BotCount,
                            engineConfig.StartRadius));
                }

                var startingFood = engineConfig.WorldFood.PlayerSafeFood;
                for (var startFoodPlaced = 0; startFoodPlaced < startingFood; startFoodPlaced++)
                {
                    var heading = GetHeadingFromSeed(playerSeed);
                    var distance = GetDistanceFromSeed(
                        playerSeed,
                        engineConfig.StartingPlayerSize + engineConfig.WorldFood.MinSeparation,
                        engineConfig.WorldFood.MaxStartingSeparation);

                    var changeVariable = playerSeed % 359;
                    playerSeed += changeVariable;

                    //To stop infinite loop if mod is 0
                    if (changeVariable == 0)
                    {
                        playerSeed += 1;
                    }

                    var foodPosition = vectorCalculatorService.GetPositionFrom(startingPositions[i], distance, heading);
                    var food = CreateFoodObjectAtPosition(Guid.NewGuid(), foodPosition);
                    var isValid = CheckPlacementValidity(
                        food,
                        placedFood,
                        startingPositions[i],
                        engineConfig.WorldFood.FoodSize * 2 + engineConfig.WorldFood.MinSeparation,
                        engineConfig.WorldFood.MaxStartingSeparation);

                    //Add another food to the list to create as this one is not valid
                    if (!isValid)
                    {
                        startingFood++;
                    }
                    else
                    {
                        gameObjects.Add(food);
                        placedFood.Add(food);
                    }
                }
            }

            return placedFood;
        }

        private GameObject FirstFoodPlacementCalculation(
            GameObject lastFood,
            int playerSeed,
            List<GameObject> gameObjects,
            List<GameObject> placedFood)
        {
            var heading = GetHeadingFromSeed(playerSeed);
            var distance = GetDistanceFromSeed(
                playerSeed,
                engineConfig.WorldFood.FoodSize * 2 + engineConfig.WorldFood.MinSeparation,
                engineConfig.WorldFood.MaxSeparation);

            var foodPosition = vectorCalculatorService.GetPositionFrom(lastFood.Position, distance, heading);
            var food = CreateFoodObjectAtPosition(Guid.NewGuid(), foodPosition);

            gameObjects.Add(food);
            placedFood.Add(food);

            return food;
        }

        public List<int> GeneratePlayerSeeds(List<BotObject> players)
        {
            var generator = new Random();

            var playerSeeds = new List<int>();

            for (var i = 0; i < engineConfig.BotCount; i++)
            {
                var seed = generator.Next(engineConfig.Seeds.MinSeed, engineConfig.Seeds.MaxSeed);
                playerSeeds.Add(seed);
            }

            return playerSeeds;
        }

        public GameObject CreateFoodObjectAtPosition(Guid id, Position position) =>
            CreateObjectAtPosition(id, position, GameObjectType.Food, engineConfig.WorldFood.FoodSize);

        public GameObject CreateObjectAtPosition(
            Guid id,
            Position position,
            GameObjectType gameObjectType,
            int size)
        {
            var gameObject = new GameObject();
            gameObject.Id = id;
            gameObject.Size = size;
            gameObject.Position = position;
            gameObject.GameObjectType = gameObjectType;

            return gameObject;
        }

        public bool CheckPlacementValidity(
            GameObject go,
            List<GameObject> otherObjects,
            Position origin,
            int minDistanceFromOtherObjects,
            int maxDistance,
            bool isPseudoObjectCheck = false)
        {
            var distanceFromOrigin = vectorCalculatorService.GetDistanceBetween(go.Position, origin);

            var isInWorldBounds = vectorCalculatorService.IsInWorldBounds(go.Position, engineConfig.MapRadius);
            var noObjectsInRange = CheckNoObjectsInRange(go, otherObjects, minDistanceFromOtherObjects);
            var isMinDistanceSpaced = isPseudoObjectCheck || distanceFromOrigin >= minDistanceFromOtherObjects;
            var isWithinMaxDistance = distanceFromOrigin <= maxDistance;

            return noObjectsInRange && isMinDistanceSpaced && isWithinMaxDistance && isInWorldBounds;
        }

        public bool CheckNoObjectsInRange(GameObject referenceObject, List<GameObject> objects, int minDistance)
        {
            var listOfObjectsInSquare = new List<GameObject>();

            var maxPosition = new Position(referenceObject.Position.X + minDistance, referenceObject.Position.Y + minDistance);

            var minPosition = new Position(referenceObject.Position.X - minDistance, referenceObject.Position.Y - minDistance);

            foreach (var gameObject in objects)
            {
                var objectPosition = gameObject.Position;
                if (minPosition.X <= objectPosition.X &&
                    objectPosition.X <= maxPosition.X &&
                    minPosition.Y <= objectPosition.Y &&
                    objectPosition.Y <= maxPosition.Y)
                {
                    listOfObjectsInSquare.Add(gameObject);
                }
            }

            if (listOfObjectsInSquare.Count != 0)
            {
                foreach (var gameObject in listOfObjectsInSquare)
                {
                    var distanceBetween = vectorCalculatorService.GetDistanceBetween(referenceObject.Position, gameObject.Position);
                    if (distanceBetween < minDistance)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public int GetDistanceFromSeed(int seed, int min, int max)
        {
            var internalSeed = seed;
            while (true)
            {
                var distance = internalSeed % max;
                if (distance > min)
                {
                    return distance;
                }

                internalSeed -= distance;

                //To stop infinite loop if mod is 0
                if (distance == 0)
                {
                    internalSeed -= 1;
                }
            }
        }

        public int GetHeadingFromSeed(int seed) => seed % 359;

        public Tuple<List<GameObject>, int> GenerateGasClouds(List<GameObject> gameObjects, int gasCloudSeed)
        {
            Tuple<List<GameObject>, int> result = GenerateWorldObstacles(
                gameObjects,
                engineConfig.GasClouds,
                GameObjectType.GasCloud,
                gasCloudSeed);
            return result;
        }

        public Tuple<List<GameObject>, int> GenerateAsteroidFields(List<GameObject> gameObjects, int asteroidFieldSeed)
        {
            Tuple<List<GameObject>, int> result = GenerateWorldObstacles(
                gameObjects,
                engineConfig.AsteroidFields,
                GameObjectType.AsteroidField,
                asteroidFieldSeed);
            return result;
        }

        private Tuple<List<GameObject>, int> GenerateWorldObstacles(
            List<GameObject> gameObjects,
            WorldObstacleConfig config,
            GameObjectType gameObjectType,
            int obstacleSeed)
        {
            /* Configurables */
            Tuple<List<Tuple<int, int, int, decimal>>, int> obstaclePositions = GetWorldObstaclePositions(
                config,
                obstacleSeed,
                config.Modular);
            var result = new Tuple<List<GameObject>, int>(new List<GameObject>(), obstaclePositions.Item2);
            var maxCount = Math.Min(obstaclePositions.Item1.Count, config.MaxCount);

            /*
             These item counts are merely the total number of obstacle nodes that have been generated, which will most likely not be a set amount.
             We can control the number of nodes that are actually added in the world with config instead to ensure we know exactly how many need to be added.
             Thoughts?
             */
            for (var i = 0; i < maxCount; i++)
            {
                Tuple<int, int, int, decimal> value = obstaclePositions.Item1[i];
                Tuple<List<Tuple<int, int, int, decimal>>, int> obstacleNodePositions = GetWorldObstaclePositions(
                    config,
                    value.Item1,
                    config.SubModular,
                    value.Item1,
                    value.Item2);
                var maxSubCount = Math.Min(obstacleNodePositions.Item1.Count, config.MaxSubCount);

                for (var j = 0; j < maxSubCount; j++)
                {
                    Tuple<int, int, int, decimal> subValue = obstacleNodePositions.Item1[j];

                    var position = new Position
                    {
                        X = subValue.Item1,
                        Y = subValue.Item2
                    };
                    var obstacleNode = CreateObjectAtPosition(Guid.NewGuid(), position, gameObjectType, subValue.Item3);

                    var noObjectsInRange = CheckNoObjectsInRange(
                        obstacleNode,
                        gameObjects.Where(g => g.GameObjectType == GameObjectType.Player).ToList(),
                        config.MinDistanceFromPlayers);

                    if (noObjectsInRange)
                    {
                        result.Item1.Add(obstacleNode);
                        gameObjects.Add(obstacleNode);
                    }
                }
            }

            return result;
        }

        private Tuple<List<Tuple<int, int, int, decimal>>, int> GetWorldObstaclePositions(
            WorldObstacleConfig config,
            decimal seed,
            int modular,
            int? offsetx = null,
            int? offsety = null)
        {
            var values = new List<Tuple<int, int, int, decimal>>();
            var multiplier = config.Multiplier;
            var xs = seed;
            var ys = seed;
            var constX = config.ConstX;
            var constY = config.ConstY;
            var constXY = config.ConstXY;

            for (var i = 0; i < config.GenerateSubCount; i++)
            {
                /* Get the size of each node as the first character of the previous nodes x and y coordinates combined. */
                var s = Convert.ToInt32(
                    Convert.ToDecimal(Math.Abs(xs).ToString(CultureInfo.InvariantCulture)[0] + "" + Math.Abs(ys).ToString(CultureInfo.InvariantCulture)[0]) * config.NodeSizeMultiplier);

                /* Calculate random seeded value for x */
                xs = xs * multiplier % modular + constX;
                var x = Convert.ToInt32((Math.Round(xs, 0) % constXY > 0 ? -1 : 1) * xs);

                /* Calculate random seeded value for y */
                ys = ys * multiplier % modular + constY;
                var y = Convert.ToInt32((Math.Round(ys, 0) % constXY > 0 ? -1 : 1) * ys);

                /* Calculate distance to center point of the circle (0,0). */
                var d = vectorCalculatorService.GetDistanceBetween(
                    new Position
                    {
                        X = x,
                        Y = y
                    },
                    new Position
                    {
                        X = 0,
                        Y = 0
                    });

                /* Only add items within the circle radius. */
                if (d <= modular)
                {
                    if (offsetx.HasValue &&
                        offsety.HasValue)
                    {
                        x += offsetx.Value;
                        y += offsety.Value;
                    }

                    values.Add(new Tuple<int, int, int, decimal>(x, y, s, d));
                }

                var repeatingValues = values.GroupBy(v => v.Item1).Where(vg => vg.Count() > config.RepeatingValuesLimit).Count();

                /* Check if duplicate pattern exists, then start over with new seed. */
                if (repeatingValues > config.RepeatingValuesLimit)
                {
                    i = 0;
                    seed = xs = ys = offsetx.HasValue || seed < config.MaxSeed ? seed + 1 : seed * -1;
                    values = new List<Tuple<int, int, int, decimal>>();
                }
            }

            return new Tuple<List<Tuple<int, int, int, decimal>>, int>(values, Convert.ToInt32(seed));
        }
    }
}