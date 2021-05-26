using System.Collections.Generic;
using System.Linq;
using Domain.Models;
using Engine.Handlers.Interfaces;

namespace Engine.Handlers.Resolvers
{
    public class CollisionHandlerResolver : ICollisionHandlerResolver
    {
        private readonly IEnumerable<ICollisionHandler> collisionHandlers;

        public CollisionHandlerResolver(IEnumerable<ICollisionHandler> collisionHandlers)
        {
            this.collisionHandlers = collisionHandlers;
        }

        public ICollisionHandler ResolveHandler(GameObject gameObject, MovableGameObject bot)
        {
            return collisionHandlers.First(handler => handler.IsApplicable(gameObject, bot));
        }
    }
}