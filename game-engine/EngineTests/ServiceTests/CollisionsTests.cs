using System.Collections.Generic;
using Domain.Enums;
using Domain.Models;
using Engine.Handlers.Collisions;
using Engine.Handlers.Interfaces;
using Engine.Handlers.Resolvers;
using Engine.Interfaces;
using Engine.Services;
using NUnit.Framework;

namespace EngineTests.ServiceTests
{
    [TestFixture]
    public class CollisionsTests : TestBase
    {
        private ICollisionService collisionService;
        private CollisionHandlerResolver collisionHandlerResolver;
        private List<ICollisionHandler> collisionHandlers;

        [SetUp]
        public new void Setup()
        {
            base.Setup();
            collisionService = new CollisionService(EngineConfigFake, WorldStateService, VectorCalculatorService);
            collisionHandlers = new List<ICollisionHandler>
            {
                new FoodCollisionHandler(WorldStateService, EngineConfigFake),
                new PlayerCollisionHandler(WorldStateService, collisionService, EngineConfigFake, VectorCalculatorService)
            };
            collisionHandlerResolver = new CollisionHandlerResolver(collisionHandlers);
        }

        [Test]
        public void GivenTwoBotsOfSameSizeCollided_WhenTick_ThenBotsBounce()
        {
            var bot1 = FakeGameObjectProvider.GetBotAt(new Position(0, 0));
            var bot2 = FakeGameObjectProvider.GetBotAt(new Position(19, 0));
            bot1.CurrentAction = new PlayerAction
            {
                Action = PlayerActions.Forward,
                Heading = 0
            };
            bot2.CurrentAction = new PlayerAction
            {
                Action = PlayerActions.Forward,
                Heading = 180
            };

            var handler = collisionHandlerResolver.ResolveHandler(bot2, bot1);
            var resultForBot1 = handler.ResolveCollision(bot2, bot1);
            var resultForBot2 = handler.ResolveCollision(bot1, bot2);

            Assert.True(resultForBot1);
            Assert.True(resultForBot2);
            Assert.AreEqual(0, bot1.Score);
            Assert.AreEqual(0, bot2.Score);
        }

        [Test]
        public void GivenTwoBotsCollided_WhenTick_ThenSmallerBotDies()
        {
            var bot1 = FakeGameObjectProvider.GetBotAt(new Position(0, 0));
            var bot2 = FakeGameObjectProvider.GetBigBotAt(new Position(19, 0));

            var handler = collisionHandlerResolver.ResolveHandler(bot2, bot1);
            var result = handler.ResolveCollision(bot2, bot1);

            Assert.False(result);
            Assert.AreEqual(EngineConfigFake.Value.ScoreRates[GameObjectType.Player], bot2.Score);
        }

        [Test]
        public void GivenTwoBotsCollided_WhenTick_ThenLargerBotLives()
        {
            var bot1 = FakeGameObjectProvider.GetBotAt(new Position(0, 0));
            var bot2 = FakeGameObjectProvider.GetBigBotAt(new Position(19, 0));

            var handler = collisionHandlerResolver.ResolveHandler(bot1, bot2);
            var result = handler.ResolveCollision(bot1, bot2);

            Assert.True(result);
            Assert.AreEqual(EngineConfigFake.Value.ScoreRates[GameObjectType.Player], bot2.Score);
        }

        [Test]
        public void GivenTwoBotsCollided_WhenTick_ThenBiggerBotIncreasesSize()
        {
            var bot1 = FakeGameObjectProvider.GetBotAt(new Position(0, 0));
            var bot2 = FakeGameObjectProvider.GetBigBotAt(new Position(19, 0));
            var originalSize = bot2.Size;

            var handler = collisionHandlerResolver.ResolveHandler(bot2, bot1);
            var result = handler.ResolveCollision(bot2, bot1);

            Assert.True(bot2.Size > originalSize);
            Assert.AreEqual(EngineConfigFake.Value.ScoreRates[GameObjectType.Player], bot2.Score);
        }

        [Test]
        public void GivenTwoBotsCollided_WhenTick_ThenBiggerBotSpeedDecreases()
        {
            var bot1 = FakeGameObjectProvider.GetBotAt(new Position(0, 0));
            var bot2 = FakeGameObjectProvider.GetBigBotAt(new Position(19, 0));
            var originalSpeed = bot2.Speed;

            var handler = collisionHandlerResolver.ResolveHandler(bot2, bot1);
            var result = handler.ResolveCollision(bot2, bot1);

            Assert.True(bot2.Speed < originalSpeed);
            Assert.AreEqual(EngineConfigFake.Value.ScoreRates[GameObjectType.Player], bot2.Score);
        }

        [Test]
        public void GivenBotCollidedWithFood_WhenTick_ThenBotSizeIncreaseBy1()
        {
            var food = FakeGameObjectProvider.GetFoodAt(new Position(0, 0));
            var bot = FakeGameObjectProvider.GetBotAt(new Position(8, 0));
            var originalSize = bot.Size;

            var handler = collisionHandlerResolver.ResolveHandler(food, bot);
            var result = handler.ResolveCollision(food, bot);

            Assert.True(bot.Size == originalSize + 1);
            Assert.AreEqual(EngineConfigFake.Value.ScoreRates[GameObjectType.Food], bot.Score);
        }

        [Test]
        public void GivenBotCollidedWithFood_WhenTick_ThenBotSpeedDecreases()
        {
            var food = FakeGameObjectProvider.GetFoodAt(new Position(0, 0));
            var bot = FakeGameObjectProvider.GetBotAt(new Position(8, 0));
            var originalSpeed = bot.Speed;

            var handler = collisionHandlerResolver.ResolveHandler(food, bot);
            var result = handler.ResolveCollision(food, bot);

            Assert.True(bot.Speed < originalSpeed);
            Assert.AreEqual(EngineConfigFake.Value.ScoreRates[GameObjectType.Food], bot.Score);
        }

        [Test]
        public void GivenBotCollidedWithFood_WhenTick_ThenFoodRemovedFromWorld()
        {
            var food = FakeGameObjectProvider.GetFoodAt(new Position(0, 0));
            var bot = FakeGameObjectProvider.GetBotAt(new Position(8, 0));
            var originalSpeed = bot.Speed;

            var handler = collisionHandlerResolver.ResolveHandler(food, bot);
            var result = handler.ResolveCollision(food, bot);

            Assert.False(WorldStateService.GameObjectIsInWorldState(food.Id));
            Assert.True(WorldStateService.GameObjectIsInWorldState(bot.Id));
            Assert.AreEqual(EngineConfigFake.Value.ScoreRates[GameObjectType.Food], bot.Score);
        }

        [Test]
        public void GivenTwoBotsCollided_WhenTick_ThenSmallerBotIsRemovedFromWorld()
        {
            var bot1 = FakeGameObjectProvider.GetBotAt(new Position(0, 0));
            var bot2 = FakeGameObjectProvider.GetBigBotAt(new Position(19, 0));

            var handler = collisionHandlerResolver.ResolveHandler(bot2, bot1);
            var result = handler.ResolveCollision(bot2, bot1);

            Assert.False(WorldStateService.GameObjectIsInWorldState(bot1.Id));
            Assert.True(WorldStateService.GameObjectIsInWorldState(bot2.Id));
            Assert.AreEqual(EngineConfigFake.Value.ScoreRates[GameObjectType.Player], bot2.Score);
        }

        [Test]
        public void GivenMovingBotCollidesWithDeadBot_WhenTick_ThenDeadBotIsRemovedFromWorld()
        {
            var deadBot = FakeGameObjectProvider.GetBotAtDefault();
            var aliveBot = FakeGameObjectProvider.GetBigBotAt(new Position(19, 0));

            var handler = collisionHandlerResolver.ResolveHandler(deadBot, aliveBot);
            var result = handler.ResolveCollision(deadBot, aliveBot);

            Assert.False(WorldStateService.GameObjectIsInWorldState(deadBot.Id));
            Assert.True(WorldStateService.GameObjectIsInWorldState(aliveBot.Id));
        }
    }
}