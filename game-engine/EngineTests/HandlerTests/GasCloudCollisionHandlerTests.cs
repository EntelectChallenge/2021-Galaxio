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
    public class GasCloudCollisionHandlerTests : TestBase
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
                new GasCloudCollisionHandler(WorldStateService, EngineConfigFake),
                new PlayerCollisionHandler(WorldStateService, collisionService, EngineConfigFake, new VectorCalculatorService())
            };
            collisionHandlerResolver = new CollisionHandlerResolver(collisionHandlers);
        }

        [Test]
        public void GivenBotAndGasCloud_WhenResolveCollision_ReturnBotWithReducedSize()
        {
            WorldStateService.GenerateStartingWorld();
            var state = WorldStateService.GetState();

            List<GameObject> gasClouds = state.GasClouds;
            var gasCloud = gasClouds[0];

            var bot = FakeGameObjectProvider.GetBotAt(gasCloud.Position);

            var handler = collisionHandlerResolver.ResolveHandler(gasCloud, bot);
            handler.ResolveCollision(gasCloud, bot);

            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());

            var activeEffect = WorldStateService.GetActiveEffectByType(bot.Id, Effects.GasCloud);
            var botAfter = WorldStateService.GetState().PlayerGameObjects.Find(g => g.Id == bot.Id);

            Assert.True(activeEffect != default);
            Assert.True(activeEffect.Effect == Effects.GasCloud);
            Assert.True(botAfter != default);
            Assert.True(botAfter.Effects == Effects.GasCloud);
            Assert.AreEqual(8, bot.Size);
        }
    }
}