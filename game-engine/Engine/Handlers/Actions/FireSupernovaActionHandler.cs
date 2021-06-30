using Domain.Enums;
using Domain.Models;
using Engine.Handlers.Interfaces;
using Engine.Interfaces;
using Engine.Models;
using Engine.Services;

namespace Engine.Handlers.Actions
{
    public class FireSupernovaActionHandler : IActionHandler
    {
        private readonly IWorldStateService worldStateService;
        private readonly IVectorCalculatorService vectorCalculatorService;
        private readonly EngineConfig engineConfig;

        public FireSupernovaActionHandler(
            IWorldStateService worldStateService,
            IVectorCalculatorService vectorCalculatorService,
            IConfigurationService configurationService)
        {
            this.worldStateService = worldStateService;
            this.vectorCalculatorService = vectorCalculatorService;
            engineConfig = configurationService.Value;
        }

        public bool IsApplicable(PlayerAction action) => action.Action == PlayerActions.FireSupernova;

        public void ProcessAction(BotObject bot)
        {
            if (bot.SupernovaAvailable == 0)
            {
                return;
            }

            var position = vectorCalculatorService.GetPositionFrom(
                bot.Position,
                bot.Size + engineConfig.Supernova.Size + 1,
                bot.CurrentAction.Heading);

            bot.SupernovaAvailable = 0;

            worldStateService.AddSupernova(bot.CurrentAction.Heading, position, bot.Id);
        }
    }
}