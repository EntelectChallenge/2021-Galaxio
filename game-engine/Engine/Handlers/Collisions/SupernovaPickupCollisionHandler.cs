using Domain.Enums;
using Domain.Models;
using Engine.Handlers.Interfaces;
using Engine.Interfaces;

namespace Engine.Handlers.Collisions
{
    public class SupernovaPickupCollisionHandler: ICollisionHandler
    {
        private readonly IWorldStateService worldStateService;

        public SupernovaPickupCollisionHandler(IWorldStateService worldStateService)
        {
            this.worldStateService = worldStateService;
        }

        public bool IsApplicable(GameObject gameObject, MovableGameObject mover) => gameObject.GameObjectType == GameObjectType.SupernovaPickup;

        public bool ResolveCollision(GameObject gameObject, MovableGameObject mover)
        {
            if (mover is BotObject bot)
            {
                var extantSupernovaPickup = worldStateService.GetObjectById(gameObject.Id);
                if (extantSupernovaPickup == null)
                {
                    return true;
                }
                bot.SupernovaAvailable++;
                worldStateService.RemoveGameObjectById(gameObject.Id);
            }

            return true;
        }
    }
}