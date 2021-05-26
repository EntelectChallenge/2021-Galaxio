using Domain.Enums;
using Domain.Models;
using Engine.Handlers.Interfaces;
using Engine.Interfaces;
using Engine.Models;
using Engine.Services;

namespace Engine.Handlers.Collisions
{
    public class SuperfoodCollisionHandler : ICollisionHandler
    {
        private readonly IWorldStateService worldStateService;
        private readonly EngineConfig engineConfig;

        public SuperfoodCollisionHandler(IWorldStateService worldStateService, IConfigurationService engineConfigOptions)
        {
            this.worldStateService = worldStateService;
            engineConfig = engineConfigOptions.Value;
        }

        public bool IsApplicable(GameObject gameObject, MovableGameObject mover) => gameObject.GameObjectType == GameObjectType.Superfood;

        public bool ResolveCollision(GameObject go, MovableGameObject mover)
        {
            // If the bot's ID has already been removed from the world, the bot is dead, return the alive state as false
            if (!worldStateService.GameObjectIsInWorldState(mover.Id))
            {
                return false;
            }

            // If the colliding GO has already been removed from the world, but we reached here, the bot is alive but need not process the GO collision
            if (!worldStateService.GameObjectIsInWorldState(go.Id))
            {
                return true;
            }

            if (mover.Size > engineConfig.WorldFood.MaxConsumptionSize)
            {
                return true;
            }

            if (mover is BotObject bot)
            {
                bot.Size += go.Size;
                bot.Score += engineConfig.ScoreRates[GameObjectType.Superfood];

                var superFoodEffect = worldStateService.GetActiveEffectByType(bot.Id, Effects.Superfood);
                if (superFoodEffect != null)
                {
                    superFoodEffect.EffectDuration += engineConfig.WorldFood.SuperfoodEffectDuration;
                }
                else
                {
                    var currentEffect = new ActiveEffect
                    {
                        Bot = bot,
                        Effect = Effects.Superfood,
                        EffectDuration = engineConfig.WorldFood.SuperfoodEffectDuration
                    };
                    worldStateService.AddActiveEffect(currentEffect);
                }

                worldStateService.UpdateBotSpeed(bot);
                go.Size = 0;
                worldStateService.RemoveGameObjectById(go.Id);
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
            }

            return true;
        }
    }
}