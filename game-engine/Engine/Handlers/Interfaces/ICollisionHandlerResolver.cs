using Domain.Models;

namespace Engine.Handlers.Interfaces
{
    public interface ICollisionHandlerResolver
    {
        ICollisionHandler ResolveHandler(GameObject gameObject, BotObject bot);
    }
}