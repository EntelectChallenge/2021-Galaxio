using System.Collections.Generic;
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
                vectorCalculatorServiceMock.Object,
                WorldStateService,
                collisionService);
        }

        [Test]
        public void GivenBot_WhenActionIsProcessed_ThenVectorIsStepwiseCalculated()
        {
            SetupFakeWorld(true, false);
            var bot = FakeGameObjectProvider.GetBotWithActions();
            bot.IsMoving = true;
            vectorCalculatorServiceMock.Setup(x => x.MovePlayerObject(It.IsAny<Position>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(VectorCalculatorService.MovePlayerObject(bot.Position, 1, bot.CurrentHeading));
            vectorCalculatorServiceMock.Setup(x => x.IsInWorldBounds(It.IsAny<Position>(), It.IsAny<int>())).Returns(true);

            Assert.DoesNotThrow(() => tickProcessingService.SimulateTick(bot));

            vectorCalculatorServiceMock.Verify(
                x => x.MovePlayerObject(It.IsAny<Position>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.AtLeast(2));
        }

        [Test]
        public void GivenBot_WhenSizeChangesDuringActionProcessing_ThenRemainingProcessStepsAreAdjusted()
        {
            SetupFakeWorld(true, false);
            var food = PlaceFoodAtPosition(new Position(0, 1));
            var bot = FakeGameObjectProvider.GetBotWithActions();
            bot.Speed = 20;
            bot.IsMoving = true;
            vectorCalculatorServiceMock.Setup(x => x.MovePlayerObject(It.IsAny<Position>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(VectorCalculatorService.MovePlayerObject(bot.Position, 1, bot.CurrentHeading));
            vectorCalculatorServiceMock.Setup(x => x.IsInWorldBounds(It.IsAny<Position>(), It.IsAny<int>())).Returns(true);

            vectorCalculatorServiceMock.Setup(x => x.HasOverlap(food, bot)).Returns(true);

            Assert.DoesNotThrow(() => tickProcessingService.SimulateTick(bot));

            vectorCalculatorServiceMock.Verify(
                x => x.MovePlayerObject(It.IsAny<Position>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.Exactly(19));

            Assert.True(bot.Size == 11);
            Assert.True(bot.Speed == 19);
        }

        [Test]
        public void GivenTwoBotsCollided_WhenTick_ThenProcessingStepsStopEarly()
        {
            var bot1 = FakeGameObjectProvider.GetBotWithActions();
            var bot2 = FakeGameObjectProvider.GetBigBotAt(new Position(0, 15));
            bot1.IsMoving = true;
            bot2.IsMoving = true;

            vectorCalculatorServiceMock.Setup(x => x.MovePlayerObject(It.IsAny<Position>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new Position());
            vectorCalculatorServiceMock.Setup(x => x.IsInWorldBounds(It.IsAny<Position>(), It.IsAny<int>())).Returns(true);
            vectorCalculatorServiceMock.Setup(x => x.HasOverlap(bot2, bot1)).Returns(true);

            Assert.DoesNotThrow(() => tickProcessingService.SimulateTick(bot1));
            Assert.False(WorldStateService.GameObjectIsInWorldState(bot1.Id));
            Assert.True(WorldStateService.GameObjectIsInWorldState(bot2.Id));

            vectorCalculatorServiceMock.Verify(x => x.MovePlayerObject(It.IsAny<Position>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }
    }
}