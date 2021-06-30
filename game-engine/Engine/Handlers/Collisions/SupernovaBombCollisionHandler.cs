using Domain.Enums;
using Domain.Models;
using Engine.Handlers.Interfaces;

namespace Engine.Handlers.Collisions
{
    public class SupernovaBombCollisionHandler: ICollisionHandler
    {
        public bool IsApplicable(GameObject gameObject, MovableGameObject mover) => gameObject.GameObjectType == GameObjectType.SupernovaBomb;

        public bool ResolveCollision(GameObject gameObject, MovableGameObject mover) => true; // no-op. Nothing collides with the bomb
    }
}