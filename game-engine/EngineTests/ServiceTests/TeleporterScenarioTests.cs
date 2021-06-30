using System.Collections.Generic;
using System.Linq;
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
    public class TeleporterScenarioTests : TestBase
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
                new TorpedoCollisionHandler(EngineConfigFake, WorldStateService, VectorCalculatorService),
                new TeleporterCollisionHandler(EngineConfigFake, WorldStateService)
            };
            collisionHandlerResolver = new CollisionHandlerResolver(collisionHandlers);
            actionHandlers = new List<IActionHandler>
            {
                new ForwardActionHandler(),
                new StartAfterburnerActionHandler(WorldStateService, EngineConfigFake),
                new StopAfterburnerActionHandler(WorldStateService),
                new StopActionHandler(),
                new FireTorpedoActionHandler(WorldStateService, VectorCalculatorService, EngineConfigFake),
                new FireTeleporterActionHandler(WorldStateService, VectorCalculatorService, EngineConfigFake),
                new TeleportActionHandler(WorldStateService),
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
        public void GivenPlayer_WhenUsesFireTeleport_ThenTeleportGameObjectGenerated()
        {
            SetupFakeWorld();
            var bot = WorldStateService.GetPlayerBots().First();
            bot.PendingActions.Add(
                new PlayerAction
                {
                    Action = PlayerActions.FireTeleport,
                    Heading = 180,
                    PlayerId = bot.Id
                });
            
            Assert.AreEqual(1, bot.TeleporterCount);
            actionService.ApplyActionToBot(bot);

            var teleporterCount = WorldStateService.GetCurrentGameObjects()
                                                .Where(obj => obj.GameObjectType == GameObjectType.Teleporter);
            Assert.IsNotEmpty(teleporterCount);
            Assert.AreEqual(0, bot.TeleporterCount);
        }
        
        [Test]
        public void GivenPlayer_WhenUsesFireTeleportAndNoTeleports_ThenTeleportGameObjectNotGenerated()
        {
            SetupFakeWorld();
            var bot = WorldStateService.GetPlayerBots().First();
            bot.TeleporterCount = 0;
            bot.PendingActions.Add(
                new PlayerAction
                {
                    Action = PlayerActions.FireTeleport,
                    Heading = 180,
                    PlayerId = bot.Id
                });
            actionService.ApplyActionToBot(bot);
            
            var teleporterCount = WorldStateService.GetCurrentGameObjects()
                .Where(obj => obj.GameObjectType == GameObjectType.Teleporter);
            Assert.IsEmpty(teleporterCount);
        }
        
        [Test]
        public void GivenPlayer_WhenTicking_ThenTeleportIncreases()
        {
            SetupFakeWorld();
            var bot = WorldStateService.GetPlayerBots().First();
            
            tickProcessingService = new TickProcessingService(
                collisionHandlerResolver,
                VectorCalculatorService,
                WorldStateService,
                collisionService);

            Assert.AreEqual(1, bot.TeleporterCount);

            Assert.DoesNotThrow(() => tickProcessingService.SimulateTick());
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());
            Assert.DoesNotThrow(() => tickProcessingService.SimulateTick());
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());
            Assert.DoesNotThrow(() => tickProcessingService.SimulateTick());
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());
            Assert.DoesNotThrow(() => tickProcessingService.SimulateTick());
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());
            Assert.DoesNotThrow(() => tickProcessingService.SimulateTick());
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());
            Assert.DoesNotThrow(() => tickProcessingService.SimulateTick());
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());
            Assert.DoesNotThrow(() => tickProcessingService.SimulateTick());
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());
            Assert.DoesNotThrow(() => tickProcessingService.SimulateTick());
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());
            Assert.DoesNotThrow(() => tickProcessingService.SimulateTick());
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());
            Assert.DoesNotThrow(() => tickProcessingService.SimulateTick());
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());

            Assert.AreEqual(2, bot.TeleporterCount);
            
            bot.PendingActions.Add(
                new PlayerAction
                {
                    Action = PlayerActions.FireTeleport,
                    Heading = 180,
                    PlayerId = bot.Id
                });
            
            actionService.ApplyActionToBot(bot);
            Assert.DoesNotThrow(() => tickProcessingService.SimulateTick());
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());
            
            var teleporterCount = WorldStateService
                .GetCurrentGameObjects().Count(obj => obj.GameObjectType == GameObjectType.Teleporter);
            Assert.AreEqual(1,teleporterCount);
            Assert.AreEqual(1, bot.TeleporterCount);
            
            bot.PendingActions.Add(
                new PlayerAction
                {
                    Action = PlayerActions.FireTeleport,
                    Heading = 180,
                    PlayerId = bot.Id
                });
            
            actionService.ApplyActionToBot(bot);
            Assert.DoesNotThrow(() => tickProcessingService.SimulateTick());
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());
            
            teleporterCount = WorldStateService
                .GetCurrentGameObjects().Count(obj => obj.GameObjectType == GameObjectType.Teleporter);
            Assert.AreEqual(2,teleporterCount);
            Assert.AreEqual(0, bot.TeleporterCount);
            
            bot.PendingActions.Add(
                new PlayerAction
                {
                    Action = PlayerActions.FireTeleport,
                    Heading = 180,
                    PlayerId = bot.Id
                });
            
            actionService.ApplyActionToBot(bot);
            Assert.DoesNotThrow(() => tickProcessingService.SimulateTick());
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());
            
            teleporterCount = WorldStateService
                .GetCurrentGameObjects().Count(obj => obj.GameObjectType == GameObjectType.Teleporter);
            Assert.AreEqual(2,teleporterCount);
            Assert.AreEqual(0, bot.TeleporterCount);
        }
    }
}