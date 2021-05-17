using Domain.Enums;
using Domain.Models;
using Engine.Handlers.Interfaces;
using Engine.Interfaces;

namespace Engine.Handlers.Collisions
{
    public class AsteroidFieldCollisionHandler : ICollisionHandler
    {
        private readonly IWorldStateService worldStateService;

        public AsteroidFieldCollisionHandler(IWorldStateService worldStateService)
        {
            this.worldStateService = worldStateService;
        }

        public bool IsApplicable(GameObject gameObject, MovableGameObject mover) => gameObject.GameObjectType == GameObjectType.AsteroidField;

        public bool ResolveCollision(GameObject go, MovableGameObject mover)
        {
            var currentEffect = new ActiveEffect
            {
                Bot = mover,
                Effect = Effects.AsteroidField
            };

            if (mover is BotObject bot)
            {
                /* If the effect is not registered, add it to the list. */
                if (worldStateService.GetActiveEffectByType(mover.Id, Effects.AsteroidField) == default)
                {
                    worldStateService.AddActiveEffect(currentEffect);
                    worldStateService.UpdateBotSpeed(mover);
                }
            }
            else
            {
                var moverStartingSize = mover.Size;
                mover.Size -= go.Size;
                go.Size -= moverStartingSize;
                if (go.Size <= 0)
                {
                    go.Size = 0;
                    worldStateService.RemoveGameObjectById(go.Id);
                }

                if (mover.Size <= 0)
                {
                    mover.Size = 0;
                    worldStateService.RemoveGameObjectById(mover.Id);
                }

                return mover.Size > 0;
            }



            return true;
        }
    }
}