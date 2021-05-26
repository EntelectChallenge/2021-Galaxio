using Domain.Models;

namespace Engine.Handlers.Interfaces
{
    public interface ICollisionHandler
    {
        bool IsApplicable(GameObject gameObject, MovableGameObject mover);
        bool ResolveCollision(GameObject gameObject, MovableGameObject mover);
    }
}