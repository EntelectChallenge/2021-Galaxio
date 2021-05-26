using Domain.Models;
using Engine.Handlers.Interfaces;

namespace Engine.Handlers.Collisions
{
    public class BotToTorpedoCollisionHandler : ICollisionHandler
    {
        public bool IsApplicable(GameObject gameObject, MovableGameObject mover) =>
            gameObject.GetType() == typeof(BotObject) && mover.GetType() == typeof(TorpedoGameObject);

        public bool ResolveCollision(GameObject gameObject, MovableGameObject mover) =>
            // no-op, bots are always moving which means this will be dealt with by the torpedo collision handler
            // and we no-op, to prevent collision penalties being applied bi-directionally
            true;
    }
}