using System;
using System.Collections.Generic;
using Domain.Models;
using Engine.Handlers.Collisions;
using Engine.Handlers.Interfaces;
using Engine.Handlers.Resolvers;
using Engine.Interfaces;
using Engine.Services;
using NUnit.Framework;

namespace EngineTests.HandlerTests
{
    [TestFixture]
    public class CollisionHandlerResolverTests : TestBase
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
                new WormholeCollisionHandler(WorldStateService, VectorCalculatorService, EngineConfigFake),
                new PlayerCollisionHandler(WorldStateService, collisionService, EngineConfigFake, new VectorCalculatorService()),
                new GasCloudCollisionHandler(WorldStateService, EngineConfigFake),
                new AsteroidFieldCollisionHandler(WorldStateService),
                new SuperfoodCollisionHandler(WorldStateService, EngineConfigFake)
            };
            collisionHandlerResolver = new CollisionHandlerResolver(collisionHandlers);
        }

        [Test]
        public void GivenBotAndBot_WhenCollision_ResolvesPlayerCollisionHandler()
        {
            var bot1 = FakeGameObjectProvider.GetBotAt(new Position(0, 0));
            var bot2 = FakeGameObjectProvider.GetBigBotAt(new Position(19, 0));

            var handler = collisionHandlerResolver.ResolveHandler(bot2, bot1);

            Assert.IsInstanceOf<PlayerCollisionHandler>(handler);
            Assert.True(handler.IsApplicable(bot2, bot1));
        }

        [Test]
        public void GivenBotAndFood_WhenCollision_ResolvesFoodCollisionHandler()
        {
            var food = FakeGameObjectProvider.GetFoodAt(new Position(0, 0));
            var bot = FakeGameObjectProvider.GetBotAt(new Position(8, 0));

            var handler = collisionHandlerResolver.ResolveHandler(food, bot);

            Assert.IsInstanceOf<FoodCollisionHandler>(handler);
            Assert.True(handler.IsApplicable(food, bot));
        }

        [Test]
        public void GivenBotAndWormhole_WhenCollision_ResolvesWormholeCollisionHandler()
        {
            List<Tuple<GameObject, GameObject>> wormholes = FakeGameObjectProvider.GetWormholes(12345678);
            var bot = FakeGameObjectProvider.GetBotAt(new Position(8, 0));
            Tuple<GameObject, GameObject> wormhole = wormholes[0];

            var handler = collisionHandlerResolver.ResolveHandler(wormhole.Item1, bot);

            Assert.IsInstanceOf<WormholeCollisionHandler>(handler);
            Assert.True(handler.IsApplicable(wormhole.Item1, bot));
        }

        [Test]
        public void GivenBotAndGasCloud_WhenCollision_ResolvesGasCloudCollisionHandler()
        {
            List<GameObject> gasClouds = FakeGameObjectProvider.GetGasClouds();
            var bot = FakeGameObjectProvider.GetBotAt(new Position(8, 0));
            var gasCloud = gasClouds[0];

            var handler = collisionHandlerResolver.ResolveHandler(gasCloud, bot);

            Assert.IsInstanceOf<GasCloudCollisionHandler>(handler);
            Assert.True(handler.IsApplicable(gasCloud, bot));
        }

        [Test]
        public void GivenBotAndAsteroidField_WhenCollision_ResolvesAsteroidFieldCollisionHandler()
        {
            List<GameObject> asteroidFields = FakeGameObjectProvider.GetAsteroidFields();
            var bot = FakeGameObjectProvider.GetBotAt(new Position(8, 0));
            var asteroidField = asteroidFields[0];

            var handler = collisionHandlerResolver.ResolveHandler(asteroidField, bot);

            Assert.IsInstanceOf<AsteroidFieldCollisionHandler>(handler);
            Assert.True(handler.IsApplicable(asteroidField, bot));
        }

        [Test]
        public void GivenBotAndSuperfood_WhenCollision_ResolvesSuperfoodCollisionHandler()
        {
            var superfood = FakeGameObjectProvider.GetSuperfoodAt(new Position(0, 0));
            var bot = FakeGameObjectProvider.GetBotAt(new Position(8, 0));

            var handler = collisionHandlerResolver.ResolveHandler(superfood, bot);

            Assert.IsInstanceOf<SuperfoodCollisionHandler>(handler);
            Assert.True(handler.IsApplicable(superfood, bot));
        }
    }
}