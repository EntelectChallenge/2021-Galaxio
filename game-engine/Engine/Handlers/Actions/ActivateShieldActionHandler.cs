using Domain.Enums;
using Domain.Models;
using Engine.Handlers.Interfaces;
using Engine.Interfaces;
using Engine.Models;
using Engine.Services;

namespace Engine.Handlers.Actions
{
    public class ActivateShieldActionHandler : IActionHandler
    {
        private readonly IWorldStateService worldStateService;
        private readonly EngineConfig engineConfig;

        public ActivateShieldActionHandler(IWorldStateService worldStateService, IConfigurationService engineConfig)
        {
            this.worldStateService = worldStateService;
            this.engineConfig = engineConfig.Value;
        }
        
        public bool IsApplicable(PlayerAction action) => action.Action == PlayerActions.ActivateShield;

        public void ProcessAction(BotObject bot)
        { 
            
            if (bot.ShieldCount < 1)
            {
                return;
            }
            
            var currentEffect = new ActiveEffect
            {
                Bot = bot,
                Effect = Effects.Shield,
                EffectDuration = engineConfig.Shield.ShieldEffectDuration
            };

            // If the effect is not registered, add the shield and add it to the list. #1#
            if (worldStateService.GetActiveEffectByType(bot.Id, Effects.Shield) == default)
            {
                worldStateService.AddActiveEffect(currentEffect);
                bot.Size -= engineConfig.Shield.Cost;
                bot.ShieldCount--;
            }
        }
    }
}