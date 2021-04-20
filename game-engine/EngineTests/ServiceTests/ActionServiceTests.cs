using System.Collections.Generic;
using Domain.Enums;
using Domain.Models;
using Engine.Handlers.Actions;
using Engine.Handlers.Interfaces;
using Engine.Handlers.Resolvers;
using Engine.Services;
using NUnit.Framework;

namespace EngineTests.ServiceTests
{
    [TestFixture]
    public class ActionServiceTests : TestBase
    {
        private ActionService actionService;
        private IActionHandlerResolver actionHandlerResolver;
        private List<IActionHandler> actionHandlers;

        [SetUp]
        public new void Setup()
        {
            base.Setup();
            actionHandlers = new List<IActionHandler>
            {
                new ForwardActionHandler(),
                new StartAfterburnerActionHandler(WorldStateService, EngineConfigFake),
                new StopAfterburnerActionHandler(WorldStateService)
            };
            actionHandlerResolver = new ActionHandlerResolver(actionHandlers);
            actionService = new ActionService(WorldStateService, actionHandlerResolver);
        }

        [Test]
        public void GivenBot_WhenBotHasNoPendingActions_ThenProcessNothing()
        {
            var bot = FakeGameObjectProvider.GetBotAtDefault();
            bot.PendingActions = null;

            Assert.DoesNotThrow(() => actionService.ApplyActionToBot(bot));
        }

        [Test]
        public void GivenBot_WithMultiplePendingActions_ThenOldestActionIsUsed()
        {
            SetupFakeWorld();
            var bot = FakeGameObjectProvider.GetBotAtDefault();
            var firstAction = FakeGameObjectProvider.GetForwardPlayerAction(bot.Id);
            bot.PendingActions = new List<PlayerAction>
            {
                firstAction,
                FakeGameObjectProvider.GetForwardPlayerAction(bot.Id),
                FakeGameObjectProvider.GetForwardPlayerAction(bot.Id)
            };
            bot.Speed = 1;

            Assert.DoesNotThrow(() => actionService.ApplyActionToBot(bot));
            Assert.False(bot.PendingActions.Contains(firstAction));
            Assert.AreEqual(firstAction, bot.CurrentAction);
        }

        [Test]
        public void GivenBot_WhenIsOnlyBotAlive_ThenProcessNothing()
        {
            SetupFakeWorld(false);
            var bot = FakeGameObjectProvider.GetBotWithActions();

            Assert.DoesNotThrow(() => actionService.ApplyActionToBot(bot));
        }

        [Test]
        public void GivenBot_WithAfterburnerNotStarted_ThenStartAfterburner()
        {
            SetupFakeWorld();
            var bot = FakeGameObjectProvider.GetBotAtDefault();
            var firstAction = FakeGameObjectProvider.GetStartAfterburnerPlayerAction(bot.Id);
            bot.PendingActions = new List<PlayerAction>
            {
                firstAction
            };

            Assert.DoesNotThrow(() => actionService.ApplyActionToBot(bot));
            var activeEffect = WorldStateService.GetActiveEffectByType(bot.Id, Effects.Afterburner);
            var botAfter = WorldStateService.GetState().PlayerGameObjects.Find(g => g.Id == bot.Id);

            Assert.True(activeEffect != default);
            Assert.True(activeEffect.Bot.Size == 10);
            Assert.True(activeEffect.Bot.Speed == 40);
            Assert.True(botAfter != default);
            Assert.True(botAfter.Effects == Effects.Afterburner);

            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());

            Assert.True(activeEffect.Bot.Size == 9);
            Assert.True(activeEffect.Bot.Speed == 40);
            Assert.True(botAfter != default);
            Assert.True(botAfter.Effects == Effects.Afterburner);
        }

        [Test]
        public void GivenBot_WithAfterburnerStarted_ThenStopAfterburner()
        {
            SetupFakeWorld();
            var bot = FakeGameObjectProvider.GetBotAtDefault();
            var firstAction = FakeGameObjectProvider.GetStartAfterburnerPlayerAction(bot.Id);
            bot.PendingActions = new List<PlayerAction>
            {
                firstAction
            };

            Assert.DoesNotThrow(() => actionService.ApplyActionToBot(bot));
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());
            
            var secondAction = FakeGameObjectProvider.GetStopAfterburnerPlayerAction(bot.Id);
            bot.PendingActions = new List<PlayerAction>
            {
                secondAction
            };

            Assert.DoesNotThrow(() => actionService.ApplyActionToBot(bot));
            var activeEffect = WorldStateService.GetActiveEffectByType(bot.Id, Effects.Afterburner);
            var botAfter = WorldStateService.GetState().PlayerGameObjects.Find(g => g.Id == bot.Id);

            Assert.True(activeEffect == default);
            Assert.AreEqual(9, bot.Size);
            Assert.AreEqual(23, bot.Speed);
            Assert.True(botAfter != default);
            Assert.True(botAfter.Effects != Effects.Afterburner);
        }

        [Test]
        public void GivenBot_WithAfterburnerStarted_ThenStartAfterburnerAgain()
        {
            SetupFakeWorld();
            var bot = FakeGameObjectProvider.GetBotAtDefault();
            var firstAction = FakeGameObjectProvider.GetStartAfterburnerPlayerAction(bot.Id);
            bot.PendingActions = new List<PlayerAction>
            {
                firstAction
            };

            Assert.DoesNotThrow(() => actionService.ApplyActionToBot(bot));
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());

            Assert.AreEqual(9, bot.Size);
            Assert.AreEqual(40, bot.Speed);

            var secondAction = FakeGameObjectProvider.GetStartAfterburnerPlayerAction(bot.Id);
            bot.PendingActions = new List<PlayerAction>
            {
                secondAction
            };

            Assert.DoesNotThrow(() => actionService.ApplyActionToBot(bot));
            var activeEffect = WorldStateService.GetActiveEffectByType(bot.Id, Effects.Afterburner);

            Assert.True(activeEffect != default);
            Assert.AreEqual(9, bot.Size);
            Assert.AreEqual(40, bot.Speed);
        }

        [Test]
        public void GivenBot_WithAfterburnerStarted_ThenProcessTwoTicks()
        {
            SetupFakeWorld();
            var bot = FakeGameObjectProvider.GetBotAtDefault();
            var firstAction = FakeGameObjectProvider.GetStartAfterburnerPlayerAction(bot.Id);
            bot.PendingActions = new List<PlayerAction>
            {
                firstAction
            };

            Assert.DoesNotThrow(() => actionService.ApplyActionToBot(bot));
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());

            var activeEffect = WorldStateService.GetActiveEffectByType(bot.Id, Effects.Afterburner);

            Assert.True(activeEffect != default);
            Assert.AreEqual(8, bot.Size);
            Assert.AreEqual(46, bot.Speed);
        }

        [Test]
        public void GivenBot_WithAfterburnerNotStarted_ThenStopAfterburner()
        {
            SetupFakeWorld();
            var bot = FakeGameObjectProvider.GetBotAtDefault();
            var firstAction = FakeGameObjectProvider.GetStopAfterburnerPlayerAction(bot.Id);
            bot.PendingActions = new List<PlayerAction>
            {
                firstAction
            };

            Assert.DoesNotThrow(() => actionService.ApplyActionToBot(bot));
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());

            var activeEffect = WorldStateService.GetActiveEffectByType(bot.Id, Effects.Afterburner);

            Assert.True(activeEffect == default);
            Assert.AreEqual(10, bot.Size);
            Assert.AreEqual(20, bot.Speed);
        }

        [Test]
        public void GivenBot_WithAfterburnerNotStarted_ThenBotIsTooSmall()
        {
            SetupFakeWorld();
            var bot = FakeGameObjectProvider.GetBotAtDefault();
            bot.Size = 5;

            var firstAction = FakeGameObjectProvider.GetStartAfterburnerPlayerAction(bot.Id);
            bot.PendingActions = new List<PlayerAction>
            {
                firstAction
            };

            Assert.DoesNotThrow(() => actionService.ApplyActionToBot(bot));
            Assert.DoesNotThrow(() => WorldStateService.ApplyAfterTickStateChanges());

            var activeEffect = WorldStateService.GetActiveEffectByType(bot.Id, Effects.Afterburner);

            Assert.True(activeEffect == default);
            Assert.AreEqual(5, bot.Size);
            Assert.AreEqual(40, bot.Speed);
        }
    }
}