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
                new PlayerCollisionHandler(WorldStateService, collisionService, EngineConfigFake, VectorCalculatorService),
                new WormholeCollisionHandler(WorldStateService, VectorCalculatorService, EngineConfigFake),
                new GasCloudCollisionHandler(WorldStateService, EngineConfigFake),
                new AsteroidFieldCollisionHandler(WorldStateService),
                new SuperfoodCollisionHandler(WorldStateService, EngineConfigFake),
                new TorpedoCollisionHandler(EngineConfigFake, WorldStateService)
            };
            collisionHandlerResolver = new CollisionHandlerResolver(collisionHandlers);
            actionHandlers = new List<IActionHandler>
            {
                new ForwardActionHandler(),
                new StartAfterburnerActionHandler(WorldStateService, EngineConfigFake),
                new StopAfterburnerActionHandler(WorldStateService),
                new StopActionHandler(),
                new FireTorpedoActionHandler(WorldStateService, VectorCalculatorService, EngineConfigFake)
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

            Assert.DoesNotThrow(() => engineService.SimulateTickForBots(WorldStateService.GetPlayerBots()));
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());
            Assert.DoesNotThrow(() => engineService.SimulateTickForBots(WorldStateService.GetPlayerBots()));
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());

            Assert.AreEqual(59, bot.Size);
            Assert.AreEqual(9, bot.Speed);
            Assert.AreEqual(0, bot.Position.Y);
            Assert.AreEqual(9, bot.Position.X);
        }

        [Test]
        public void GivenBot_WithMovement_AndAfterburnerStarted_ThenDistanceAndSpeedCorrect()
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

            Assert.DoesNotThrow(() => engineService.SimulateTickForBots(WorldStateService.GetPlayerBots()));
            ;
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());
            Assert.DoesNotThrow(() => engineService.SimulateTickForBots(WorldStateService.GetPlayerBots()));
            ;
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());

            var activeEffect = WorldStateService.GetActiveEffectByType(bot.Id, Effects.Afterburner);

            Assert.AreEqual(59, bot.Size);
            Assert.AreEqual(9, bot.Speed);
            Assert.AreEqual(0, bot.Position.Y);
            Assert.AreEqual(13, bot.Position.X);

            Assert.DoesNotThrow(() => engineService.SimulateTickForBots(WorldStateService.GetPlayerBots()));
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());

            Assert.AreEqual(58, bot.Size);
            Assert.AreEqual(9, bot.Speed);
            Assert.AreEqual(0, bot.Position.Y);
            Assert.AreEqual(22, bot.Position.X);
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

            List<Position> pathPoints =
                VectorCalculatorService.CollectCollisionDetectionPointsAlongPath(new Position(0, 0), new Position(6, 2), 23);

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

            for (var j = 0; j < 6; j++)
            {
                Assert.DoesNotThrow(() => actionService.ApplyActionToBot(bot));
                Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());
            }

            Assert.AreEqual(4, bot.Size);
            Assert.False(WorldStateService.GameObjectIsInWorldState(bot.Id));
        }

        [Test]
        public void GivenBotAndSuperfood_WhenResolveCollision_ReturnBotWithActiveEffects()
        {
            SetupFakeWorld(true, false);
            FakeGameObjectProvider.GetSuperfoodAt(new Position(8, 0));
            FakeGameObjectProvider.GetFoodAt(new Position(80, 0));

            var bot = FakeGameObjectProvider.GetBotAt(new Position(0, 0));
            var firstAction = FakeGameObjectProvider.GetForwardPlayerAction(bot.Id);

            bot.PendingActions = new List<PlayerAction>
            {
                firstAction
            };

            Assert.DoesNotThrow(() => engineService.SimulateTickForBots(WorldStateService.GetPlayerBots()));
            ;
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());

            var activeEffect = WorldStateService.GetActiveEffectByType(bot.Id, Effects.Superfood);
            var botAfter = WorldStateService.GetState().PlayerGameObjects.Find(g => g.Id == bot.Id);

            Assert.True(activeEffect != default);
            Assert.True(activeEffect.Effect == Effects.Superfood);
            Assert.True(botAfter != default);
            Assert.True(botAfter.Effects == Effects.Superfood);
            Assert.AreEqual(11, bot.Size);

            Assert.DoesNotThrow(() => engineService.SimulateTickForBots(WorldStateService.GetPlayerBots()));
            ;
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());
            Assert.DoesNotThrow(() => engineService.SimulateTickForBots(WorldStateService.GetPlayerBots()));
            ;
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());
            Assert.DoesNotThrow(() => engineService.SimulateTickForBots(WorldStateService.GetPlayerBots()));
            ;
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());
            Assert.DoesNotThrow(() => engineService.SimulateTickForBots(WorldStateService.GetPlayerBots()));
            ;
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());
            Assert.DoesNotThrow(() => engineService.SimulateTickForBots(WorldStateService.GetPlayerBots()));
            ;
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());

            activeEffect = WorldStateService.GetActiveEffectByType(bot.Id, Effects.Superfood);
            botAfter = WorldStateService.GetState().PlayerGameObjects.Find(g => g.Id == bot.Id);

            Assert.True(activeEffect == null);
            Assert.True(botAfter != default);
            Assert.AreEqual(13, bot.Size);
        }
    }
}