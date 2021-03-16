using System;
using System.Collections.Generic;
using Domain.Enums;
using Domain.Models;
using Engine.Handlers.Actions;
using Engine.Handlers.Collisions;
using Engine.Handlers.Interfaces;
using Engine.Handlers.Resolvers;
using Engine.Services;
using NUnit.Framework;

namespace EngineTests.ServiceTests
{
    [TestFixture]
    public class PlayerScenarioTests : TestBase
    {
        private ActionService actionService;
        private CollisionService collisionService;
        private CollisionHandlerResolver collisionHandlerResolver;
        private List<ICollisionHandler> collisionHandlers;
        private IActionHandlerResolver actionHandlerResolver;
        private List<IActionHandler> actionHandlers;
        private EngineService engineService;
        private TickProcessingService tickProcessingService;

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
            actionHandlers = new List<IActionHandler>
            {
                new ForwardActionHandler(),
                new StartAfterburnerActionHandler(WorldStateService, EngineConfigFake),
                new StopAfterburnerActionHandler(WorldStateService)
            };
            actionHandlerResolver = new ActionHandlerResolver(actionHandlers);
            actionService = new ActionService(WorldStateService, actionHandlerResolver);
            tickProcessingService = new TickProcessingService(
                collisionHandlerResolver,
                VectorCalculatorService,
                WorldStateService,
                collisionService);
            engineService = new EngineService(WorldStateService, actionService, EngineConfigFake, tickProcessingService);
        }

        [Test]
        public void GivenBot_WithAfterburnerStarted_ThenStopAfterburnerAndNormalSpeedCorrect()
        {
            SetupFakeWorld(true, false);
            var food = FakeGameObjectProvider.GetFoodAt(new Position(60, 0));
            var bot = new BotObject
            {
                Id = Guid.NewGuid(),
                Size = 60,
                Position = new Position(0, 0),
                Speed = 4,
                GameObjectType = GameObjectType.Player
            };

            WorldStateService.AddBotObject(bot);

            var firstAction = FakeGameObjectProvider.GetStartAfterburnerPlayerAction(bot.Id);
            var secondAction = FakeGameObjectProvider.GetForwardPlayerAction(bot.Id);

            bot.PendingActions = new List<PlayerAction>
            {
                firstAction,
                secondAction
            };

            engineService.ProcessTickForBot(bot);
            WorldStateService.ApplyAfterTickStateChanges();
            Assert.DoesNotThrow(() => engineService.ProcessTickForBot(bot));
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());

            Assert.AreEqual(59, bot.Size);
            Assert.AreEqual(8, bot.Speed);
            Assert.AreEqual(0, bot.Position.Y);
            Assert.AreEqual(8, bot.Position.X);
        }

        [Test]
        public void GivenBot_WithMovement_AndAfterburnerStarted_ThenStopAfterburnerAndNormalSpeedCorrect()
        {
            SetupFakeWorld(true, false);
            var bot = new BotObject
            {
                Id = Guid.NewGuid(),
                Size = 60,
                Position = new Position(0, 0),
                Speed = 4,
                GameObjectType = GameObjectType.Player
            };
            WorldStateService.AddBotObject(bot);

            var firstAction = FakeGameObjectProvider.GetForwardPlayerAction(bot.Id);
            var secondAction = FakeGameObjectProvider.GetStartAfterburnerPlayerAction(bot.Id);
            var thirdAction = FakeGameObjectProvider.GetForwardPlayerAction(bot.Id);

            bot.PendingActions = new List<PlayerAction>
            {
                firstAction,
                secondAction,
                thirdAction
            };

            engineService.ProcessTickForBot(bot);
            WorldStateService.ApplyAfterTickStateChanges();
            Assert.DoesNotThrow(() => engineService.ProcessTickForBot(bot));
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());

            var activeEffect = WorldStateService.GetActiveEffectByType(bot.Id, Effects.Afterburner);

            Assert.AreEqual(59, bot.Size);
            Assert.AreEqual(8, bot.Speed);
            Assert.AreEqual(0, bot.Position.Y);
            Assert.AreEqual(12, bot.Position.X);

            Assert.DoesNotThrow(() => engineService.ProcessTickForBot(bot));
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());

            Assert.AreEqual(58, bot.Size);
            Assert.AreEqual(8, bot.Speed);
            Assert.AreEqual(0, bot.Position.Y);
            Assert.AreEqual(20, bot.Position.X);
        }
    }
}