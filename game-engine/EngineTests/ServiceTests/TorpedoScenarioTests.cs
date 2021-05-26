using System;
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
    public class TorpedoScenarioTests : TestBase
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
        public void GivenTorpedoes_WhenHitFood_ThenSmallerAndTorpedoSmaller()
        {
            SetupFakeWorld();
            var food = FakeGameObjectProvider.GetFoodAt(new Position(0, 10));

            var torpedo = new TorpedoGameObject
            {
                Id = Guid.NewGuid(),
                GameObjectType = GameObjectType.TorpedoSalvo,
                Size = EngineConfigFake.Value.Torpedo.Size,
                IsMoving = true,
                Position = food.Position
            };
            WorldStateService.AddGameObject(torpedo);

            var handler = collisionHandlerResolver.ResolveHandler(food, torpedo);
            handler.ResolveCollision(food, torpedo);

            Assert.IsInstanceOf<FoodCollisionHandler>(handler);
            Assert.True(torpedo.Size == torpedo.Size - food.Size);
        }

        [Test]
        public void GivenTorpedoes_WhenHitWormhole_ThenTorpedoTraversesWormhole()
        {
            WorldStateService.GenerateStartingWorld();
            var state = WorldStateService.GetState();

            List<Tuple<GameObject, GameObject>> wormholes = state.WormholePairs;
            var wormhole = wormholes[0].Item1;

            var startPosition = new Position(wormhole.Position.X + wormhole.Size + 5, wormhole.Position.Y + wormhole.Size + 5);
            var torpedo = new TorpedoGameObject
            {
                Id = Guid.NewGuid(),
                GameObjectType = GameObjectType.TorpedoSalvo,
                Size = EngineConfigFake.Value.Torpedo.Size,
                IsMoving = true,
                Position = startPosition
            };
            WorldStateService.AddGameObject(torpedo);

            var handler = collisionHandlerResolver.ResolveHandler(wormhole, torpedo);
            handler.ResolveCollision(wormhole, torpedo);

            Assert.IsInstanceOf<WormholeCollisionHandler>(handler);
            Assert.True(torpedo.Position != startPosition);
        }

        [Test]
        public void GivenTorpedoes_WhenHitSuperFood_ThenSmallerAndTorpedoSmaller()
        {
            SetupFakeWorld();
            var superFood = FakeGameObjectProvider.GetSuperfoodAt(new Position(0, 10));

            var torpedo = new TorpedoGameObject
            {
                Id = Guid.NewGuid(),
                GameObjectType = GameObjectType.TorpedoSalvo,
                Size = EngineConfigFake.Value.Torpedo.Size,
                IsMoving = true,
                Position = superFood.Position
            };
            WorldStateService.AddGameObject(torpedo);

            var handler = collisionHandlerResolver.ResolveHandler(superFood, torpedo);
            handler.ResolveCollision(superFood, torpedo);

            Assert.IsInstanceOf<SuperfoodCollisionHandler>(handler);
            Assert.True(torpedo.Size == torpedo.Size - superFood.Size);
        }

        [Test]
        public void GivenTorpedoes_WhenHitGasCloud_ThenSmallerAndTorpedoSmaller()
        {
            SetupFakeWorld();
            List<GameObject> gasClouds = FakeGameObjectProvider.GetGasClouds();
            var gasCloud = gasClouds[0];

            var torpedo = new TorpedoGameObject
            {
                Id = Guid.NewGuid(),
                GameObjectType = GameObjectType.TorpedoSalvo,
                Size = EngineConfigFake.Value.Torpedo.Size,
                IsMoving = true,
                Position = new Position(8, 0)
            };
            WorldStateService.AddGameObject(torpedo);

            var handler = collisionHandlerResolver.ResolveHandler(gasCloud, torpedo);
            handler.ResolveCollision(gasCloud, torpedo);

            Assert.IsInstanceOf<GasCloudCollisionHandler>(handler);
            Assert.Less(torpedo.Size, 10);
        }

        [Test]
        public void GivenTorpedoes_WhenHitAsteroid_ThenSmallerAndTorpedoSmaller()
        {
            SetupFakeWorld();
            var superFood = FakeGameObjectProvider.GetSuperfoodAt(new Position(0, 10));
            List<GameObject> asteroidFields = FakeGameObjectProvider.GetAsteroidFields();
            var asteroidField = asteroidFields[0];
            var asteroidSize = asteroidField.Size;

            var torpedo = new TorpedoGameObject
            {
                Id = Guid.NewGuid(),
                GameObjectType = GameObjectType.TorpedoSalvo,
                Size = EngineConfigFake.Value.Torpedo.Size,
                IsMoving = true,
                Position = new Position(8, 0)
            };
            WorldStateService.AddGameObject(torpedo);

            var handler = collisionHandlerResolver.ResolveHandler(asteroidField, torpedo);
            handler.ResolveCollision(asteroidField, torpedo);

            Assert.IsInstanceOf<AsteroidFieldCollisionHandler>(handler);
            Assert.Less(torpedo.Size, 10);
            Assert.Less(asteroidField.Size, asteroidSize);
        }

        [Test]
        public void GivenTorpedoes_WhenHitPlayer_ThenSmallerAndTorpedoSmaller_AndFiringPlayerLarger()
        {
            SetupFakeWorld();
            var bot = WorldStateService.GetPlayerBots().First();
            var playerBot = FakeGameObjectProvider.GetBotAt(new Position(100, 100));
            var torpedo = new TorpedoGameObject
            {
                Id = Guid.NewGuid(),
                GameObjectType = GameObjectType.TorpedoSalvo,
                Size = EngineConfigFake.Value.Torpedo.Size,
                IsMoving = true,
                Position = new Position(0, 10),
                Speed = EngineConfigFake.Value.Torpedo.Speed,
                FiringPlayerId = playerBot.Id
            };
            WorldStateService.AddGameObject(torpedo);

            var handler = collisionHandlerResolver.ResolveHandler(torpedo, bot);
            handler.ResolveCollision(torpedo, bot);

            Assert.IsInstanceOf<TorpedoCollisionHandler>(handler);
            Assert.AreEqual(0, torpedo.Size);
            Assert.AreEqual(0, bot.Size);
            Assert.AreEqual(20, playerBot.Size);
        }

        [Test]
        public void GivenPlayer_WhenUsesFireTorpedoes_ThenTorpedoGameObjectGenerated()
        {
            SetupFakeWorld();
            var bot = WorldStateService.GetPlayerBots().First();
            bot.PendingActions.Add(
                new PlayerAction
                {
                    Action = PlayerActions.FireTorpedoes,
                    Heading = 180,
                    PlayerId = bot.Id
                });
            actionService.ApplyActionToBot(bot);

            Assert.IsNotEmpty(WorldStateService.GetMovableObjects().Where(obj => obj.GameObjectType == GameObjectType.TorpedoSalvo));
        }
    }
}