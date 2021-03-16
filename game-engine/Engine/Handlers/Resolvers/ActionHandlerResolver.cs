using System.Collections.Generic;
using System.Linq;
using Domain.Models;
using Engine.Handlers.Interfaces;

namespace Engine.Handlers.Resolvers
{
    public class ActionHandlerResolver : IActionHandlerResolver
    {
        private readonly IEnumerable<IActionHandler> handlers;

        public ActionHandlerResolver(IEnumerable<IActionHandler> handlers)
        {
            this.handlers = handlers;
        }

        public IActionHandler ResolveHandler(PlayerAction botCurrentAction)
        {
            return handlers.First(handler => handler.IsApplicable(botCurrentAction));
        }
    }
}