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
    public class TeleporterCollisionHandlerTests : TestBase
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
                new TeleporterCollisionHandler(EngineConfigFake, WorldStateService),
                new PlayerCollisionHandler(WorldStateService, collisionService, EngineConfigFake, new VectorCalculatorService())
            };
            collisionHandlerResolver = new CollisionHandlerResolver(collisionHandlers);
        }

        [Test]
        public void GivenBotAndTeleporter_WhenResolveCollision_ReturnSamePositions()
        {
            WorldStateService.GenerateStartingWorld();

            var bot = FakeGameObjectProvider.GetBotAt(
                new Position(0,0));
            
            var teleporter = FakeGameObjectProvider.GetTeleporter(
                new Position(0,0), 0, Guid.NewGuid());

            var handler = collisionHandlerResolver.ResolveHandler(teleporter, bot);
            handler.ResolveCollision(teleporter, bot);
            

            Assert.AreEqual(teleporter.Position.X, bot.Position.X);
            Assert.AreEqual(teleporter.Position.Y, bot.Position.Y);
        }
    }
}