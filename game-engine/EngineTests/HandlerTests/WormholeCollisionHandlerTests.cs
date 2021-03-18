using System;
using System.Collections.Generic;
using Domain.Enums;
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
    public class WormholeCollisionHandlerTests : TestBase
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
                new PlayerCollisionHandler(WorldStateService, collisionService, EngineConfigFake, new VectorCalculatorService())
            };
            collisionHandlerResolver = new CollisionHandlerResolver(collisionHandlers);
        }

        [Test]
        public void GivenBotAndWormhole_WhenResolveCollision_ReturnBotInNewPositionAndShrunkWormholes()
        {
            WorldStateService.GenerateStartingWorld();
            var state = WorldStateService.GetState();

            List<Tuple<GameObject, GameObject>> wormholes = state.WormholePairs;
            var wormhole = wormholes[0].Item1;
            var wormholeSize = wormhole.Size;

            var bot = FakeGameObjectProvider.GetBotAt(
                new Position(wormhole.Position.X + wormhole.Size + 5, wormhole.Position.Y + wormhole.Size + 5));

            var handler = collisionHandlerResolver.ResolveHandler(wormhole, bot);
            handler.ResolveCollision(wormhole, bot);

            var expectedPosition = VectorCalculatorService.GetPositionFrom(
                wormholes[0].Item2.Position,
                wormholeSize + bot.Size,
                bot.CurrentHeading);

            Assert.AreEqual(expectedPosition.X, bot.Position.X);
            Assert.AreEqual(expectedPosition.Y, bot.Position.Y);
            Assert.AreEqual(EngineConfigFake.Value.ScoreRates[GameObjectType.Wormhole], bot.Score);

            Assert.True(state.WormholePairs[0].Item1.Size < wormholeSize);
            Assert.True(state.WormholePairs[0].Item2.Size < wormholeSize);
            Assert.True(state.WormholePairs[0].Item2.Size == state.WormholePairs[0].Item1.Size);
        }
    }
}