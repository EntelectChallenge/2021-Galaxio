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
    public class AsteroidFieldCollisionHandlerTests : TestBase
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
                new AsteroidFieldCollisionHandler(WorldStateService),
                new PlayerCollisionHandler(WorldStateService, collisionService, EngineConfigFake, new VectorCalculatorService())
            };
            collisionHandlerResolver = new CollisionHandlerResolver(collisionHandlers);
        }

        [Test]
        public void GivenBotAndAsteroidField_WhenResolveCollision_ReturnBotWithReducedSize()
        {
            WorldStateService.GenerateStartingWorld();
            var state = WorldStateService.GetState();

            List<GameObject> asteroidFields = state.AsteroidFields;
            var asteroidField = asteroidFields[0];

            var bot = FakeGameObjectProvider.GetBotAt(asteroidField.Position);

            var handler = collisionHandlerResolver.ResolveHandler(asteroidField, bot);
            handler.ResolveCollision(asteroidField, bot);

            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());

            var activeEffect = WorldStateService.GetActiveEffectByType(bot.Id, Effects.AsteroidField);
            var botAfter = WorldStateService.GetState().PlayerGameObjects.Find(g => g.Id == bot.Id);

            Assert.True(activeEffect != default);
            Assert.True(activeEffect.Effect == Effects.AsteroidField);
            Assert.True(botAfter != default);
            Assert.True(botAfter.Effects == Effects.AsteroidField);
            Assert.AreEqual(19, bot.Speed);
        }

        [Test]
        public void GivenBot_WhenUpdateBotSpeedWithBigBot_ReturnOne()
        {
            WorldStateService.GenerateStartingWorld();
            var state = WorldStateService.GetState();
            state.World.Radius = 1900;

            List<GameObject> asteroidFields = state.AsteroidFields;
            var asteroidField = asteroidFields[0];

            var bot = FakeGameObjectProvider.GetBotAt(
                new Position(asteroidField.Position.X + asteroidField.Size + 5, asteroidField.Position.Y + asteroidField.Size + 5));

            bot.Size = 340;
            bot.Speed = 1;
            
            var handler = collisionHandlerResolver.ResolveHandler(asteroidField, bot);
            handler.ResolveCollision(asteroidField, bot);
            
            Assert.AreEqual(1, bot.Speed);
        }
    }
}