using Domain.Models;
using Engine.Services;
using NUnit.Framework;

namespace EngineTests.ServiceTests
{
    [TestFixture]
    public class VectorCalculationServiceTests : TestBase
    {
        private VectorCalculatorService vectorCalculatorService;

        [SetUp]
        public new void Setup()
        {
            base.Setup();
            vectorCalculatorService = new VectorCalculatorService();
        }

        [Test]
        public void GivenA0DegreeHeading_WhenMoveAction_ThenMoveRightBySpeed()
        {
            var bot = FakeGameObjectProvider.GetBotAt(new Position(0, 0));
            var action = FakeGameObjectProvider.GetForwardPlayerActionInHeading(bot.Id, 0);

            var finalPosition = vectorCalculatorService.MovePlayerObject(new Position(0, 0), 10, action.Heading);

            Assert.True(finalPosition.X == 10);
            Assert.True(finalPosition.Y == 0);
        }

        [Test]
        public void GivenA180DegreeHeading_WhenMoveAction_ThenMoveLeftBySpeed()
        {
            var bot = FakeGameObjectProvider.GetBotAt(new Position(0, 0));
            var action = FakeGameObjectProvider.GetForwardPlayerActionInHeading(bot.Id, 180);

            var finalPosition = vectorCalculatorService.MovePlayerObject(new Position(0, 0), 10, action.Heading);

            Assert.True(finalPosition.X == -10);
            Assert.True(finalPosition.Y == 0);
        }

        [Test]
        public void GivenA90DegreeHeading_WhenMoveAction_ThenMoveUpBySpeed()
        {
            var bot = FakeGameObjectProvider.GetBotAt(new Position(0, 0));
            var action = FakeGameObjectProvider.GetForwardPlayerActionInHeading(bot.Id, 90);

            var finalPosition = vectorCalculatorService.MovePlayerObject(new Position(0, 0), 10, action.Heading);

            Assert.True(finalPosition.X == 0);
            Assert.True(finalPosition.Y == 10);
        }

        [Test]
        public void GivenA270DegreeHeading_WhenMoveAction_ThenMoveDownBySpeed()
        {
            var bot = FakeGameObjectProvider.GetBotAt(new Position(0, 0));
            var action = FakeGameObjectProvider.GetForwardPlayerActionInHeading(bot.Id, 270);

            var finalPosition = vectorCalculatorService.MovePlayerObject(new Position(0, 0), 10, action.Heading);

            Assert.True(finalPosition.X == 0);
            Assert.True(finalPosition.Y == -10);
        }

        [Test]
        public void GivenA45DegreeHeading_WhenMoveAction_ThenMiddleQ1()
        {
            var bot = FakeGameObjectProvider.GetBotAt(new Position(0, 0));
            var action = FakeGameObjectProvider.GetForwardPlayerActionInHeading(bot.Id, 45);

            var finalPosition = vectorCalculatorService.MovePlayerObject(new Position(0, 0), 10, action.Heading);

            Assert.True(finalPosition.X == 7);
            Assert.True(finalPosition.Y == 7);
        }

        [Test]
        public void GivenA135DegreeHeading_WhenMoveAction_ThenMiddleQ2()
        {
            var bot = FakeGameObjectProvider.GetBotAt(new Position(0, 0));
            var action = FakeGameObjectProvider.GetForwardPlayerActionInHeading(bot.Id, 135);

            var finalPosition = vectorCalculatorService.MovePlayerObject(new Position(0, 0), 10, action.Heading);

            Assert.True(finalPosition.X == -7);
            Assert.True(finalPosition.Y == 7);
        }

        [Test]
        public void GivenA225DegreeHeading_WhenMoveAction_ThenMiddleQ3()
        {
            var bot = FakeGameObjectProvider.GetBotAt(new Position(0, 0));
            var action = FakeGameObjectProvider.GetForwardPlayerActionInHeading(bot.Id, 225);

            var finalPosition = vectorCalculatorService.MovePlayerObject(new Position(0, 0), 10, action.Heading);

            Assert.True(finalPosition.X == -7);
            Assert.True(finalPosition.Y == -7);
        }

        [Test]
        public void GivenA225DegreeHeading_WhenMoveAction_ThenMiddleQ4()
        {
            var bot = FakeGameObjectProvider.GetBotAt(new Position(0, 0));
            var action = FakeGameObjectProvider.GetForwardPlayerActionInHeading(bot.Id, 315);

            var finalPosition = vectorCalculatorService.MovePlayerObject(new Position(0, 0), 10, action.Heading);

            Assert.True(finalPosition.X == 7);
            Assert.True(finalPosition.Y == -7);
        }

        [Test]
        public void GivenHeading45AndRadius20_WhenGetStartPosition_ThenReturnStartPosition()
        {
            var startPosition = vectorCalculatorService.GetStartPosition(20, 45);

            Assert.True(startPosition.X == 14);
            Assert.True(startPosition.Y == 14);
        }

        [Test]
        public void GivenHeading135AndRadius30_WhenGetStartPosition_ThenReturnStartPosition()
        {
            var startPosition = vectorCalculatorService.GetStartPosition(30, 135);

            Assert.True(startPosition.X == -21);
            Assert.True(startPosition.Y == 21);
        }

        [Test]
        public void GivenTwoObjectNotTouching_WhenHasOverlap_ThenReturnFalse()
        {
            var bot1 = FakeGameObjectProvider.GetBotAt(new Position(0, 0));
            var bot2 = FakeGameObjectProvider.GetBotAt(new Position(40, 0));

            var hasOverlap = vectorCalculatorService.HasOverlap(bot1, bot2);

            Assert.False(hasOverlap);
        }

        [Test]
        public void GivenTwoObjectsTouching_WhenHasOverlap_ThenReturnFalse()
        {
            var bot1 = FakeGameObjectProvider.GetBotAt(new Position(0, 0));
            var bot2 = FakeGameObjectProvider.GetBigBotAt(new Position(30, 0));

            var hasOverlap = vectorCalculatorService.HasOverlap(bot1, bot2);

            Assert.False(hasOverlap);
        }

        [Test]
        public void GivenTwoObjectOverlap_WhenHasOverlap_ThenReturnTrue()
        {
            var bot1 = FakeGameObjectProvider.GetBotAt(new Position(18, 0));
            var bot2 = FakeGameObjectProvider.GetBotAt(new Position(0, 0));

            var hasOverlap = vectorCalculatorService.HasOverlap(bot1, bot2);

            Assert.True(hasOverlap);
        }

        [Test]
        public void GivenTwoWithLargeObjectOverlap_WhenHasOverlap_ThenReturnTrue()
        {
            var bot1 = FakeGameObjectProvider.GetBigBotAt(new Position(0, 0));
            var bot2 = FakeGameObjectProvider.GetBotAt(new Position(10, 0));

            var hasOverlap = vectorCalculatorService.HasOverlap(bot1, bot2);

            Assert.True(hasOverlap);
        }

        [Test]
        public void GivenObjectInWorldMap_WhenIsInWorldBounds_ThenReturnTrue()
        {
            var botPosition = new Position(300, 218);

            var inWorld = vectorCalculatorService.IsInWorldBounds(botPosition, 1000);

            Assert.True(inWorld);
        }

        [Test]
        public void GivenObjectOutsideWorldMap_WhenIsInWorldBounds_ThenReturnFalse()
        {
            var botPosition = new Position(2831, 1832);

            var inWorld = vectorCalculatorService.IsInWorldBounds(botPosition, 1000);

            Assert.False(inWorld);
        }

        [Test]
        public void GivenObjectOnEdgeOfWorldMap_WhenIsInWorldBounds_ThenReturnTrue()
        {
            var botPosition = new Position(1000, 0);

            var inWorld = vectorCalculatorService.IsInWorldBounds(botPosition, 1000);

            Assert.True(inWorld);
        }

        [Test]
        public void GivenTwoPositions_WhenGetDistanceBetween_ThenReturnDistance20()
        {
            var bot1 = FakeGameObjectProvider.GetBotAt(new Position(0, 0));
            var bot2 = FakeGameObjectProvider.GetBotAt(new Position(20, 0));

            var distanceBetween = vectorCalculatorService.GetDistanceBetween(bot1.Position, bot2.Position);

            Assert.True(distanceBetween == 20);
        }

        [Test]
        public void GivenTwoPositionsQ1andQ1_WhenGetDistanceBetween_ThenReturnDistance10()
        {
            var bot1 = FakeGameObjectProvider.GetBotAt(new Position(0, 0));
            var bot2 = FakeGameObjectProvider.GetBotAt(new Position(7, 7));

            var distanceBetween = vectorCalculatorService.GetDistanceBetween(bot1.Position, bot2.Position);

            Assert.True(distanceBetween == 10);
        }

        [Test]
        public void GivenTwoPositionsQ1andQ2_WhenGetDistanceBetween_ThenReturnDistance10()
        {
            var bot1 = FakeGameObjectProvider.GetBotAt(new Position(0, 0));
            var bot2 = FakeGameObjectProvider.GetBotAt(new Position(-7, 7));

            var distanceBetween = vectorCalculatorService.GetDistanceBetween(bot1.Position, bot2.Position);

            Assert.True(distanceBetween == 10);
        }

        [Test]
        public void GivenTwoPositionsQ1andQ3_WhenGetDistanceBetween_ThenReturnDistance10()
        {
            var bot1 = FakeGameObjectProvider.GetBotAt(new Position(0, 0));
            var bot2 = FakeGameObjectProvider.GetBotAt(new Position(-7, -7));

            var distanceBetween = vectorCalculatorService.GetDistanceBetween(bot1.Position, bot2.Position);

            Assert.True(distanceBetween == 10);
        }

        [Test]
        public void GivenTwoPositionsQ1andQ4_WhenGetDistanceBetween_ThenReturnDistance10()
        {
            var bot1 = FakeGameObjectProvider.GetBotAt(new Position(0, 0));
            var bot2 = FakeGameObjectProvider.GetBotAt(new Position(7, -7));

            var distanceBetween = vectorCalculatorService.GetDistanceBetween(bot1.Position, bot2.Position);

            Assert.True(distanceBetween == 10);
        }
    }
}