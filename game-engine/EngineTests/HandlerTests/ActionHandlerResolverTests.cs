using System;
using System.Collections.Generic;
using Domain.Enums;
using Domain.Models;
using Engine.Handlers.Actions;
using Engine.Handlers.Interfaces;
using Engine.Handlers.Resolvers;
using NUnit.Framework;

namespace EngineTests.HandlerTests
{
    [TestFixture]
    public class ActionHandlerResolverTests: TestBase
    {
        private ActionHandlerResolver actionHandlerResolver;
        private ForwardActionHandler forwardActionHandler;
        private NoOpActionHandler noOpActionHandler;

        [SetUp]
        public new void Setup()
        {
            base.Setup();
            forwardActionHandler = new ForwardActionHandler();
            noOpActionHandler = new NoOpActionHandler();
            actionHandlerResolver = new ActionHandlerResolver(new List<IActionHandler>{ forwardActionHandler, new StopActionHandler(), noOpActionHandler});
        }

        [Test]
        public void GivenAction_WhenResolveActionHandler_ThenResolvesSuccessfully()
        {
            var action = FakeGameObjectProvider.GetForwardPlayerAction(Guid.NewGuid());
            var handler = actionHandlerResolver.ResolveHandler(action);

            Assert.NotNull(handler);
            Assert.AreEqual(forwardActionHandler, handler);
        }

        [Test]
        public void GivenUnrecognisedAction_WhenResolveActionHandler_ThenResolvesSuccessfully()
        {
            var action = new PlayerAction
            {
                Action = (PlayerActions)9999,
                Heading = 0,
                PlayerId = Guid.NewGuid()
            };

            var handler = actionHandlerResolver.ResolveHandler(action);

            Assert.NotNull(handler);
            Assert.AreEqual(typeof(NoOpActionHandler), handler.GetType());
        }
    }
}