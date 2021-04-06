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
                new StopAfterburnerActionHandler(WorldStateService),
                new StopActionHandler()
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
            var id = Guid.NewGuid();
            var bot = new BotObject
            {
                Id = id,
                Size = 60,
                Position = new Position(0, 0),
                Speed = 4,
                GameObjectType = GameObjectType.Player,
                PendingActions = new List<PlayerAction>(),
                CurrentAction = new PlayerAction
                {
                    Action = PlayerActions.Stop,
                    Heading = 0,
                    PlayerId = id
                },
                Score = 0
            };

            WorldStateService.AddBotObject(bot);

            var firstAction = FakeGameObjectProvider.GetStartAfterburnerPlayerAction(bot.Id);
            var secondAction = FakeGameObjectProvider.GetForwardPlayerAction(bot.Id);

            bot.PendingActions = new List<PlayerAction>
            {
                firstAction,
                secondAction
            };

            Assert.DoesNotThrow(() => engineService.SimulateTickForBots(WorldStateService.GetPlayerBots()));;
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());
            Assert.DoesNotThrow(() => engineService.SimulateTickForBots(WorldStateService.GetPlayerBots()));;
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

            Assert.DoesNotThrow(() => engineService.SimulateTickForBots(WorldStateService.GetPlayerBots()));;
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());
            Assert.DoesNotThrow(() => engineService.SimulateTickForBots(WorldStateService.GetPlayerBots()));;
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());

            var activeEffect = WorldStateService.GetActiveEffectByType(bot.Id, Effects.Afterburner);

            Assert.AreEqual(59, bot.Size);
            Assert.AreEqual(8, bot.Speed);
            Assert.AreEqual(0, bot.Position.Y);
            Assert.AreEqual(12, bot.Position.X);

            Assert.DoesNotThrow(() => engineService.SimulateTickForBots(WorldStateService.GetPlayerBots()));
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());

            Assert.AreEqual(58, bot.Size);
            Assert.AreEqual(8, bot.Speed);
            Assert.AreEqual(0, bot.Position.Y);
            Assert.AreEqual(20, bot.Position.X);
        }

        [Test]
        public void GivenForwardAction_WhenHeadingIs45AndSpeedIs6_ThenDistanceTraveledIs6()
        {
            SetupFakeWorld(true, false);
            var bot = FakeGameObjectProvider.GetBotAt(new Position(0, 0));
            bot.PendingActions = new List<PlayerAction>
            {
                new PlayerAction
                {
                    Action = PlayerActions.Forward,
                    Heading = 45,
                    PlayerId = bot.Id
                }
            };
            bot.Speed = 6;

            var expectedEndpoint = new Position(4, 4);
            var expectedDistance = VectorCalculatorService.GetDistanceBetween(bot.Position, expectedEndpoint);

            Assert.DoesNotThrow(() => engineService.SimulateTickForBots(WorldStateService.GetPlayerBots()));
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());

            var resultingDistanceTravelled = VectorCalculatorService.GetDistanceBetween(new Position(0, 0), bot.Position);
            var varianceBetweenExpectedAndActualEndpoint = VectorCalculatorService.GetDistanceBetween(bot.Position, expectedEndpoint);

            Assert.AreEqual(6, expectedDistance);

            Assert.AreEqual(expectedEndpoint, bot.Position);
            Assert.AreEqual(expectedDistance, resultingDistanceTravelled);
            Assert.Zero(varianceBetweenExpectedAndActualEndpoint);
        }

        [Test]
        public void GivenForwardAction_WhenHeadingIs18AndSpeedIs6_ThenDistanceTraveledIs6()
        {
            SetupFakeWorld(true, false);
            var bot = FakeGameObjectProvider.GetBotAt(new Position(0, 0));
            bot.PendingActions = new List<PlayerAction>
            {
                new PlayerAction
                {
                    Action = PlayerActions.Forward,
                    Heading = 23,
                    PlayerId = bot.Id
                }
            };
            bot.Speed = 6;

            var expectedEndpoint = new Position(6, 2);
            var expectedDistance = VectorCalculatorService.GetDistanceBetween(bot.Position, expectedEndpoint);

            Assert.DoesNotThrow(() => engineService.SimulateTickForBots(WorldStateService.GetPlayerBots()));
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());

            var resultingDistanceTravelled = VectorCalculatorService.GetDistanceBetween(new Position(0, 0), bot.Position);
            var varianceBetweenExpectedAndActualEndpoint = VectorCalculatorService.GetDistanceBetween(bot.Position, expectedEndpoint);

            var pathPoints = VectorCalculatorService.CollectCollisionDetectionPointsAlongPath(new Position(0, 0), new Position(6, 2), 23);

            Assert.AreEqual(6, expectedDistance);

            Assert.AreEqual(expectedEndpoint, bot.Position);
            Assert.AreEqual(expectedDistance, resultingDistanceTravelled);
            Assert.Zero(varianceBetweenExpectedAndActualEndpoint);
        }
        
        [Test]
        public void GivenDeadBot_WithAfterburnerStarted_ThenRemoveFromWorldWhenSizeLessThan5()
        {
            SetupFakeWorld();
            var bot = FakeGameObjectProvider.GetBotAtDefault();
            var firstAction = FakeGameObjectProvider.GetStartAfterburnerPlayerAction(bot.Id);
            bot.PendingActions = new List<PlayerAction>
            {
                firstAction
            };

            for (var j = 0; j < 7; j++)
            {
                Assert.DoesNotThrow(() => actionService.ApplyActionToBot(bot));
                Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());
            }
            
            Assert.AreEqual(3, bot.Size);
            Assert.False(WorldStateService.GameObjectIsInWorldState(bot.Id));
        }
    }
}