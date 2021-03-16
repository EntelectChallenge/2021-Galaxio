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

        public bool IsApplicable(GameObject gameObject, BotObject bot) => gameObject.GameObjectType == GameObjectType.AsteroidField;

        public bool ResolveCollision(GameObject gameObject, BotObject bot)
        {
            var currentEffect = new ActiveEffect
            {
                Bot = bot,
                Effect = Effects.AsteroidField
            };

            /* If the effect is not registered, add it to the list. */
            if (worldStateService.GetActiveEffectByType(bot.Id, Effects.AsteroidField) == default)
            {
                worldStateService.AddActiveEffect(currentEffect);
                worldStateService.UpdateBotSpeed(bot);
            }

            return true;
        }
    }
}