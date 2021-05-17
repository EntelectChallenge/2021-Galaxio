using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Models;
using Engine.Handlers.Collisions;
using Engine.Handlers.Interfaces;
using Engine.Handlers.Resolvers;
using Engine.Interfaces;
using Engine.Services;
using Moq;
using NUnit.Framework;

namespace EngineTests.ServiceTests
{
    [TestFixture]
    public class TickProcessingServiceTests : TestBase
    {
        private Mock<IVectorCalculatorService> vectorCalculatorServiceMock;
        private CollisionService collisionService;
        private CollisionHandlerResolver collisionHandlerResolver;
        private List<ICollisionHandler> collisionHandlers;
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
            vectorCalculatorServiceMock = new Mock<IVectorCalculatorService>();
            tickProcessingService = new TickProcessingService(
                collisionHandlerResolver,
                VectorCalculatorService,
                WorldStateService,
                collisionService);
        }

        [Test]
        public void GivenBot_WhenActionIsProcessed_ThenVectorIsStepwiseCalculated()
        {
            SetupFakeWorld(true, false);
            var bot = FakeGameObjectProvider.GetBotWithActions();
            bot.IsMoving = true;
            vectorCalculatorServiceMock.Setup(x => x.GetPointFrom(It.IsAny<Position>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(VectorCalculatorService.GetPointFrom(bot.Position, 1, bot.CurrentHeading));
            vectorCalculatorServiceMock.Setup(x => x.IsInWorldBounds(It.IsAny<Position>(), It.IsAny<int>())).Returns(true);
            vectorCalculatorServiceMock
                .Setup(vcs => vcs.CollectCollisionDetectionPointsAlongPath(It.IsAny<Position>(), It.IsAny<Position>(), It.IsAny<int>()))
                .Returns(
                    new List<Position>{
                        new Position(0, 0),
                        new Position(1, 1),
                        new Position(2, 2)
                    });
            Assert.DoesNotThrow(() => tickProcessingService.SimulateTick());
        }

        [Test]
        public void GivenBot_WhenSizeChangesDuringActionProcessing_ThenRemainingProcessStepsAreAdjusted()
        {
            SetupFakeWorld(true, false);
            var food = PlaceFoodAtPosition(new Position(0, 1));
            var bot = FakeGameObjectProvider.GetBotWithActions();
            bot.CurrentHeading = 90;
            bot.Speed = 20;
            bot.IsMoving = true;

            tickProcessingService = new TickProcessingService(
                collisionHandlerResolver,
                VectorCalculatorService,
                WorldStateService,
                collisionService);

            Assert.DoesNotThrow(() => tickProcessingService.SimulateTick());

            Assert.AreEqual(11, bot.Size);
            Assert.AreEqual(19, bot.Speed);
            Assert.AreEqual(new Position(0,19),bot.Position);
        }

        [Test]
        public void GivenTwoBotsCollided_WhenTick_ThenProcessingStepsStopEarly()
        {
            var bot1 = FakeGameObjectProvider.GetBotWithActions();
            var bot2 = FakeGameObjectProvider.GetBigBotAt(new Position(0, 15));
            bot1.IsMoving = true;
            bot2.IsMoving = true;

            Assert.DoesNotThrow(() => tickProcessingService.SimulateTick());
            Assert.False(WorldStateService.GameObjectIsInWorldState(bot1.Id));
            Assert.True(WorldStateService.GameObjectIsInWorldState(bot2.Id));
        }

        [Test]
        public void GivenTorpedo_WhenProcessTick_ThenTorpedoMoves()
        {
            SetupFakeWorld(true, false);
            var bot = FakeGameObjectProvider.GetBotWithActions();
            bot.CurrentHeading = 90;
            bot.Speed = 20;
            bot.IsMoving = true;

            var torpedoPosition = VectorCalculatorService.GetPositionFrom(bot.Position, bot.Size + EngineConfigFake.Value.Torpedo.Size + 1, bot.CurrentAction.Heading);
            var torpedoSalvo = new TorpedoGameObject()
            {
                Id = Guid.NewGuid(),
                Position = torpedoPosition,
                Size = EngineConfigFake.Value.Torpedo.Size,
                Speed = EngineConfigFake.Value.Torpedo.Speed,
                CurrentHeading = 45,
                FiringPlayerId = bot.Id,
                IsMoving = true
            };
            WorldStateService.AddGameObject(torpedoSalvo);

            tickProcessingService = new TickProcessingService(
                collisionHandlerResolver,
                VectorCalculatorService,
                WorldStateService,
                collisionService);

            Assert.DoesNotThrow(() => tickProcessingService.SimulateTick());

            Assert.AreNotEqual(torpedoPosition, torpedoSalvo.Position);
        }

        [Test]
        public void GivenTorpedo_WhenProcessMultipleTicks_ThenTorpedoMoves()
        {
            SetupFakeWorld(true, false);
            var bot = FakeGameObjectProvider.GetBotWithActions();
            bot.CurrentHeading = 90;
            bot.Speed = 20;
            bot.IsMoving = true;

            var torpedoPosition = VectorCalculatorService.GetPositionFrom(bot.Position, bot.Size + EngineConfigFake.Value.Torpedo.Size + 1, bot.CurrentAction.Heading);
            var torpedoSalvo = new TorpedoGameObject()
            {
                Id = Guid.NewGuid(),
                Position = torpedoPosition,
                Size = EngineConfigFake.Value.Torpedo.Size,
                Speed = EngineConfigFake.Value.Torpedo.Speed,
                CurrentHeading = 45,
                FiringPlayerId = bot.Id,
                IsMoving = true
            };
            WorldStateService.AddGameObject(torpedoSalvo);

            tickProcessingService = new TickProcessingService(
                collisionHandlerResolver,
                VectorCalculatorService,
                WorldStateService,
                collisionService);

            Assert.DoesNotThrow(() => tickProcessingService.SimulateTick());

            Assert.AreNotEqual(torpedoPosition, torpedoSalvo.Position);

            var lastPosition = new Position(torpedoSalvo.Position);

            tickProcessingService = new TickProcessingService(
                collisionHandlerResolver,
                VectorCalculatorService,
                WorldStateService,
                collisionService);

            Assert.DoesNotThrow(() => tickProcessingService.SimulateTick());

            Assert.AreNotEqual(lastPosition, torpedoSalvo.Position);
        }
    }
}