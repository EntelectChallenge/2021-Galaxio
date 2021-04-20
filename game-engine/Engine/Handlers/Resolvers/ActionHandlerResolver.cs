using System.Collections.Generic;
using System.Linq;
using Domain.Models;
using Engine.Handlers.Actions;
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
            var handler = handlers.FirstOrDefault(h => h.IsApplicable(botCurrentAction));
            if (handler == null)
            {
                return new NoOpActionHandler();
            }

            return handler;
        }
    }
}