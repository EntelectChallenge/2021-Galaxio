using Domain.Models;

namespace Engine.Handlers.Interfaces
{
    public interface ICollisionHandler
    {
        bool IsApplicable(GameObject gameObject, BotObject bot);
        bool ResolveCollision(GameObject gameObject, BotObject bot);
    }
}