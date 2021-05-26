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
    public class SuperfoodCollisionHandlerTests : TestBase
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
                new SuperfoodCollisionHandler(WorldStateService, EngineConfigFake),
                new PlayerCollisionHandler(WorldStateService, collisionService, EngineConfigFake, new VectorCalculatorService())
            };
            collisionHandlerResolver = new CollisionHandlerResolver(collisionHandlers);
        }

        [Test]
        public void GivenBotAndSuperfood_WhenResolveCollision_ReturnBotWithActiveEffects()
        {
            WorldStateService.GenerateStartingWorld();
            var state = WorldStateService.GetState();
            var superfood = FakeGameObjectProvider.GetSuperfoodAt(new Position(0, 10));

            var bot = FakeGameObjectProvider.GetBotAt(superfood.Position);

            var handler = collisionHandlerResolver.ResolveHandler(superfood, bot);
            handler.ResolveCollision(superfood, bot);

            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());

            var activeEffect = WorldStateService.GetActiveEffectByType(bot.Id, Effects.Superfood);
            var botAfter = WorldStateService.GetState().PlayerGameObjects.Find(g => g.Id == bot.Id);

            Assert.True(activeEffect != default);
            Assert.True(activeEffect.Effect == Effects.Superfood);
            Assert.True(botAfter != default);
            Assert.True(botAfter.Effects == Effects.Superfood);
            Assert.AreEqual(11, bot.Size);
        }
    }
}