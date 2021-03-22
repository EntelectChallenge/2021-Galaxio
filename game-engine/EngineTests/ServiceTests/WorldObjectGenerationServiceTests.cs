using System;
using System.Collections.Generic;
using Domain.Enums;
using Domain.Models;
using Engine.Models;
using Engine.Services;
using NUnit.Framework;

namespace EngineTests.ServiceTests
{
    [TestFixture]
    public class WorldObjectGenerationServiceTests : TestBase
    {
        private WorldObjectGenerationService worldObjectGenerationService;

        [SetUp]
        public new void Setup()
        {
            base.Setup();
            worldObjectGenerationService = new WorldObjectGenerationService(EngineConfigFake, VectorCalculatorService);
        }

        [Test]
        public void GivenListenGameObjects_WhenGenerateWormholes_ThenReturnListOfWormholePairs()
        {
            var gameObjects = new List<GameObject>();
            var wormholeSeed = EngineConfigFake.Value.Wormholes.Seed ??
                new Random().Next(EngineConfigFake.Value.Wormholes.MinSeed, EngineConfigFake.Value.Wormholes.MaxSeed);

            List<Tuple<GameObject, GameObject>> wormholePairs = worldObjectGenerationService.GenerateWormholes(gameObjects, wormholeSeed);

            Assert.AreEqual(EngineConfigFake.Value.Wormholes.Count / 2, wormholePairs.Count);
            Assert.AreEqual(EngineConfigFake.Value.Wormholes.Count, gameObjects.Count);

            foreach (Tuple<GameObject, GameObject> pair in wormholePairs)
            {
                Tuple<GameObject, GameObject> mainPair = pair;
                foreach (Tuple<GameObject, GameObject> internalPair in wormholePairs)
                {
                    if (mainPair.Equals(internalPair))
                    {
                        continue;
                    }

                    var distanceBetween1 = VectorCalculatorService.GetDistanceBetween(mainPair.Item1.Position, internalPair.Item1.Position);
                    var distanceBetween2 = VectorCalculatorService.GetDistanceBetween(mainPair.Item1.Position, internalPair.Item2.Position);
                    var distanceBetween3 = VectorCalculatorService.GetDistanceBetween(mainPair.Item2.Position, internalPair.Item1.Position);
                    var distanceBetween4 = VectorCalculatorService.GetDistanceBetween(mainPair.Item2.Position, internalPair.Item2.Position);
                    var distanceBetween5 = VectorCalculatorService.GetDistanceBetween(mainPair.Item1.Position, mainPair.Item2.Position);
                    var distanceBetween6 = VectorCalculatorService.GetDistanceBetween(
                        internalPair.Item1.Position,
                        internalPair.Item2.Position);
                    Assert.IsTrue(
                        distanceBetween1 >= EngineConfigFake.Value.Wormholes.MinSeparation + EngineConfigFake.Value.Wormholes.MaxSize * 2);
                    Assert.IsTrue(
                        distanceBetween2 >= EngineConfigFake.Value.Wormholes.MinSeparation + EngineConfigFake.Value.Wormholes.MaxSize * 2);
                    Assert.IsTrue(
                        distanceBetween3 >= EngineConfigFake.Value.Wormholes.MinSeparation + EngineConfigFake.Value.Wormholes.MaxSize * 2);
                    Assert.IsTrue(
                        distanceBetween4 >= EngineConfigFake.Value.Wormholes.MinSeparation + EngineConfigFake.Value.Wormholes.MaxSize * 2);
                    Assert.IsTrue(
                        distanceBetween5 >= EngineConfigFake.Value.Wormholes.MinSeparation + EngineConfigFake.Value.Wormholes.MaxSize * 2);
                    Assert.IsTrue(
                        distanceBetween6 >= EngineConfigFake.Value.Wormholes.MinSeparation + EngineConfigFake.Value.Wormholes.MaxSize * 2);
                }
            }
        }

        [Test]
        public void GivenWorldSetup_WhenPlaceWormholeWithOrigin_ThenReturnGameObject()
        {
            var gameObjects = new List<GameObject>();
            var wormholeSeed = 4648964;
            var worldCenter = new Position(0, 0);
            var maxPlacementDistance = EngineConfigFake.Value.MapRadius;

            for (var j = 0; j < EngineConfigFake.Value.Wormholes.Count / 2; j++)
            {
                var wormhole = worldObjectGenerationService.PlaceWormholeWithOrigin(
                    gameObjects,
                    ref wormholeSeed,
                    worldCenter,
                    maxPlacementDistance,
                    0,
                    true);
                var distanceBetween = VectorCalculatorService.GetDistanceBetween(worldCenter, wormhole.Position);
                Assert.True(distanceBetween <= maxPlacementDistance);
            }

            Assert.AreEqual(EngineConfigFake.Value.Wormholes.Count / 2, gameObjects.Count);
        }

        [Test]
        public void GivenPlayerSeedsAndStartingFood_WhenGenerateWorldFood_ThenReturnListOfWorldFood()
        {
            var playerSeeds = new List<int>
            {
                12354789,
                58228,
                656846,
                7108040
            };

            List<GameObject> startingFoodList =
                worldObjectGenerationService.GeneratePlayerStartingFood(playerSeeds, new List<GameObject>());
            List<GameObject> placedFoodList =
                worldObjectGenerationService.GenerateWorldFood(startingFoodList, playerSeeds, new List<GameObject>());

            var foodPerOrigin =
                (EngineConfigFake.Value.WorldFood.StartingFoodCount -
                    EngineConfigFake.Value.BotCount * EngineConfigFake.Value.WorldFood.PlayerSafeFood) /
                (EngineConfigFake.Value.BotCount * EngineConfigFake.Value.WorldFood.PlayerSafeFood);
            for (var j = 0; j <= playerSeeds.Count; j++)
            {
                for (var i = foodPerOrigin * j; i < foodPerOrigin * (j + 1) - 1; i++)
                {
                    var distanceBetween = VectorCalculatorService.GetDistanceBetween(
                        placedFoodList[i].Position,
                        placedFoodList[i + 1].Position);
                    Assert.IsTrue(
                        EngineConfigFake.Value.WorldFood.FoodSize * 2 + EngineConfigFake.Value.WorldFood.MinSeparation <= distanceBetween);
                    Assert.IsTrue(distanceBetween <= EngineConfigFake.Value.WorldFood.MaxSeparation);
                }
            }

            Assert.AreEqual(
                EngineConfigFake.Value.WorldFood.StartingFoodCount -
                EngineConfigFake.Value.BotCount * EngineConfigFake.Value.WorldFood.PlayerSafeFood,
                placedFoodList.Count + 4);
        }

        [Test]
        public void GivenListOfOtherPlayerSeedsAndEmptyList_WhenGeneratePlayerStartingFood_ThenReturnListOfFood()
        {
            var positions = new List<Position>();
            for (var i = 0; i < EngineConfigFake.Value.BotCount; i++)
            {
                positions.Add(
                    VectorCalculatorService.GetNewPlayerStartingPosition(
                        i,
                        EngineConfigFake.Value.BotCount,
                        EngineConfigFake.Value.StartRadius));
            }

            var playerSeeds = new List<int>
            {
                90990803,
                5646848,
                8880741,
                26164989
            };

            List<GameObject> foodList = worldObjectGenerationService.GeneratePlayerStartingFood(playerSeeds, new List<GameObject>());

            for (var i = 0; i < foodList.Count; i++)
            {
                var playerIndex = (int) Math.Floor((double)i / EngineConfigFake.Value.WorldFood.PlayerSafeFood);
                var distanceBetween = VectorCalculatorService.GetDistanceBetween(positions[playerIndex], foodList[i].Position);
                Assert.IsTrue(
                    EngineConfigFake.Value.StartingPlayerSize + EngineConfigFake.Value.WorldFood.MinSeparation <= distanceBetween);
                Assert.IsTrue(distanceBetween <= EngineConfigFake.Value.WorldFood.MaxSeparation);
            }

            Assert.AreEqual(EngineConfigFake.Value.WorldFood.PlayerSafeFood * EngineConfigFake.Value.BotCount, foodList.Count);
        }

        [Test]
        public void GivenListOf4PlayerSeedsAndEmptyList_WhenGeneratePlayerStartingFood_ThenReturnListOfFood()
        {
            var positions = new List<Position>();
            for (var i = 0; i < EngineConfigFake.Value.BotCount; i++)
            {
                positions.Add(
                    VectorCalculatorService.GetNewPlayerStartingPosition(
                        i,
                        EngineConfigFake.Value.BotCount,
                        EngineConfigFake.Value.StartRadius));
            }

            var playerSeeds = new List<int>
            {
                12354789,
                58228,
                656846,
                7108040
            };

            List<GameObject> foodList = worldObjectGenerationService.GeneratePlayerStartingFood(playerSeeds, new List<GameObject>());

            for (int i = 0; i < foodList.Count; i++)
            {
                var playerIndex = (int) Math.Floor((double)i / EngineConfigFake.Value.WorldFood.PlayerSafeFood);
                var distanceBetween = VectorCalculatorService.GetDistanceBetween(positions[playerIndex], foodList[i].Position);
                Assert.IsTrue(
                    EngineConfigFake.Value.StartingPlayerSize + EngineConfigFake.Value.WorldFood.MinSeparation <= distanceBetween);
                Assert.IsTrue(distanceBetween <= EngineConfigFake.Value.WorldFood.MaxSeparation);
            }

            Assert.AreEqual(EngineConfigFake.Value.WorldFood.PlayerSafeFood * EngineConfigFake.Value.BotCount, foodList.Count);
        }

        [Test]
        public void GivenNumberOfBots_WhenGeneratePlayerSeeds_ThenReturnListOfSeeds()
        {
            var players = new List<BotObject>();
            for (var i = 0; i < EngineConfigFake.Value.BotCount; i++)
            {
                players.Add(new BotObject());
            }

            List<int> seeds = worldObjectGenerationService.GeneratePlayerSeeds(players);

            foreach (var seed in seeds)
            {
                Assert.IsTrue(EngineConfigFake.Value.Seeds.MinSeed <= seed && seed <= EngineConfigFake.Value.Seeds.MaxSeed);
            }
        }

        [Test]
        public void GivenGUIDAndPosition_WhenCreateFoodObjectAtPosition_ThenReturnGameObject()
        {
            var position = new Position(13, 103);
            var guid = new Guid("17acfd2d-7bb9-449f-8f6e-6bc0ac3e8501");

            var gameObject = worldObjectGenerationService.CreateFoodObjectAtPosition(guid, position);

            Assert.AreEqual(13, gameObject.Position.X);
            Assert.AreEqual(103, gameObject.Position.Y);
            Assert.AreEqual(GameObjectType.Food, gameObject.GameObjectType);
        }

        [Test]
        public void GivenHigherOutOfRangeDistance_WhenCheckPlacementValidity_ThenReturnFalse()
        {
            var foodPosition = new Position(0, 0);
            var otherPosition = new Position(0, 20);
            var food = FakeGameObjectProvider.GetFoodAt(foodPosition);
            var playerSeeds = new List<int>
            {
                12354789,
                58228,
                656846,
                7108040
            };

            List<GameObject> startingFoodList =
                worldObjectGenerationService.GeneratePlayerStartingFood(playerSeeds, new List<GameObject>());
            List<GameObject> placedFoodList =
                worldObjectGenerationService.GenerateWorldFood(startingFoodList, playerSeeds, new List<GameObject>());

            var isValid = worldObjectGenerationService.CheckPlacementValidity(food, placedFoodList, otherPosition, 1, 10);

            Assert.IsFalse(isValid);
        }

        [Test]
        public void GivenLowerOutOfRangeDistance_WhenCheckPlacementValidity_ThenReturnFalse()
        {
            var foodPosition = new Position(0, 0);
            var otherPosition = new Position(0, 2);
            var food = FakeGameObjectProvider.GetFoodAt(foodPosition);
            var playerSeeds = new List<int>
            {
                12354789,
                58228,
                656846,
                7108040
            };

            List<GameObject> startingFoodList =
                worldObjectGenerationService.GeneratePlayerStartingFood(playerSeeds, new List<GameObject>());
            List<GameObject> placedFoodList =
                worldObjectGenerationService.GenerateWorldFood(startingFoodList, playerSeeds, new List<GameObject>());

            var isValid = worldObjectGenerationService.CheckPlacementValidity(food, placedFoodList, otherPosition, 3, 10);

            Assert.IsFalse(isValid);
        }

        [Test]
        public void GivenInRangeDistance_WhenCheckPlacementValidity_ThenReturnTrue()
        {
            var foodPosition = new Position(0, 0);
            var otherPosition = new Position(0, 20);
            var food = FakeGameObjectProvider.GetFoodAt(foodPosition);
            var playerSeeds = new List<int>
            {
                12354789,
                58228,
                656846,
                7108040
            };

            List<GameObject> startingFoodList =
                worldObjectGenerationService.GeneratePlayerStartingFood(playerSeeds, new List<GameObject>());
            List<GameObject> placedFoodList =
                worldObjectGenerationService.GenerateWorldFood(startingFoodList, playerSeeds, new List<GameObject>());

            var isValid = worldObjectGenerationService.CheckPlacementValidity(food, placedFoodList, otherPosition, 3, 30);

            Assert.IsTrue(isValid);
        }

        [Test]
        public void GivenOnLowerBoundDistance_WhenCheckPlacementValidity_ThenReturnTrue()
        {
            var foodPosition = new Position(0, 0);
            var otherPosition = new Position(0, 3);
            var food = FakeGameObjectProvider.GetFoodAt(foodPosition);
            var playerSeeds = new List<int>
            {
                12354789,
                58228,
                656846,
                7108040
            };

            List<GameObject> startingFoodList =
                worldObjectGenerationService.GeneratePlayerStartingFood(playerSeeds, new List<GameObject>());
            List<GameObject> placedFoodList =
                worldObjectGenerationService.GenerateWorldFood(startingFoodList, playerSeeds, new List<GameObject>());

            var isValid = worldObjectGenerationService.CheckPlacementValidity(food, placedFoodList, otherPosition, 3, 30);

            Assert.IsTrue(isValid);
        }

        [Test]
        public void GivenOnUpperBoundDistance_WhenCheckPlacementValidity_ThenReturnTrue()
        {
            var foodPosition = new Position(0, 0);
            var otherPosition = new Position(0, 30);
            var food = FakeGameObjectProvider.GetFoodAt(foodPosition);
            var playerSeeds = new List<int>
            {
                12354789,
                58228,
                656846,
                7108040
            };

            List<GameObject> startingFoodList =
                worldObjectGenerationService.GeneratePlayerStartingFood(playerSeeds, new List<GameObject>());
            List<GameObject> placedFoodList =
                worldObjectGenerationService.GenerateWorldFood(startingFoodList, playerSeeds, new List<GameObject>());

            var isValid = worldObjectGenerationService.CheckPlacementValidity(food, placedFoodList, otherPosition, 3, 30);

            Assert.IsTrue(isValid);
        }

        [Test]
        public void GivenNoneInRange_WhenCheckNoObjectsInRange_ThenReturnTrue()
        {
            var refObject = FakeGameObjectProvider.GetFoodAt(new Position(0, 0));
            var objects = new List<GameObject>
            {
                FakeGameObjectProvider.GetFoodAt(new Position(11, 2)),
                FakeGameObjectProvider.GetFoodAt(new Position(12, 10)),
                FakeGameObjectProvider.GetFoodAt(new Position(12, 14)),
                FakeGameObjectProvider.GetFoodAt(new Position(0, 11))
            };

            var noObjectsInRange = worldObjectGenerationService.CheckNoObjectsInRange(refObject, objects, 10);

            Assert.True(noObjectsInRange);
        }

        [Test]
        public void GivenSomeInRange_WhenGetObjectsInRangeSquare_ThenReturnFalse()
        {
            var refObject = FakeGameObjectProvider.GetFoodAt(new Position(0, 0));
            var objects = new List<GameObject>
            {
                FakeGameObjectProvider.GetFoodAt(new Position(11, 2)),
                FakeGameObjectProvider.GetFoodAt(new Position(8, 5)),
                FakeGameObjectProvider.GetFoodAt(new Position(12, 14)),
                FakeGameObjectProvider.GetFoodAt(new Position(0, 11))
            };

            var noObjectsInRange = worldObjectGenerationService.CheckNoObjectsInRange(refObject, objects, 10);

            Assert.False(noObjectsInRange);
        }

        [Test]
        public void GivenAllInRange_WhenGetObjectsInRangeSquare_ThenReturnFalse()
        {
            var refObject = FakeGameObjectProvider.GetFoodAt(new Position(0, 0));
            var objects = new List<GameObject>
            {
                FakeGameObjectProvider.GetFoodAt(new Position(10, 2)),
                FakeGameObjectProvider.GetFoodAt(new Position(2, 9)),
                FakeGameObjectProvider.GetFoodAt(new Position(1, 1)),
                FakeGameObjectProvider.GetFoodAt(new Position(8, 3))
            };

            var noObjectsInRange = worldObjectGenerationService.CheckNoObjectsInRange(refObject, objects, 10);

            Assert.False(noObjectsInRange);
        }

        [Test]
        public void GivenSeedAndMinAndMax_WhenGetDistanceFromSeed_ThenReturnDistance14()
        {
            var distance = worldObjectGenerationService.GetDistanceFromSeed(210934, 1, 20);

            Assert.AreEqual(14, distance);
        }

        [Test]
        public void GivenSeedAndMinAndMax_WhenGetDistanceFromSeed_ThenReturnDistance99()
        {
            var distance = worldObjectGenerationService.GetDistanceFromSeed(210934, 40, 100);

            Assert.AreEqual(99, distance);
        }

        [Test]
        public void GivenSeedAndMinAndMax_WhenGetDistanceFromSeed_ThenReturnDistance3112()
        {
            var distance = worldObjectGenerationService.GetDistanceFromSeed(12009, 2671, 3113);

            Assert.AreEqual(3112, distance);
        }

        [Test]
        public void GivenSeed_WhenGetHeadingFromSeed_ThenReturnHeading327()
        {
            var heading = worldObjectGenerationService.GetHeadingFromSeed(1234569);

            Assert.AreEqual(327, heading);
        }

        [Test]
        public void GivenSeed_WhenGetHeadingFromSeed_ThenReturnHeading51()
        {
            var heading = worldObjectGenerationService.GetHeadingFromSeed(20873);

            Assert.AreEqual(51, heading);
        }

        [Test]
        public void GivenSeed_WhenGetHeadingFromSeed_ThenReturnHeading262()
        {
            var heading = worldObjectGenerationService.GetHeadingFromSeed(980);

            Assert.AreEqual(262, heading);
        }
    }
}