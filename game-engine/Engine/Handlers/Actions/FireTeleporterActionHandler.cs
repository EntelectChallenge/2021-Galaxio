using Domain.Enums;
using Domain.Models;
using Engine.Handlers.Interfaces;
using Engine.Interfaces;
using Engine.Models;
using Engine.Services;

namespace Engine.Handlers.Actions
{
    public class FireTeleporterActionHandler : IActionHandler
    {
        private readonly IWorldStateService worldStateService;
        private readonly IVectorCalculatorService vectorCalculatorService;
        private readonly EngineConfig engineConfig;

        public FireTeleporterActionHandler(
            IWorldStateService worldStateService,
            IVectorCalculatorService vectorCalculatorService,
            IConfigurationService configurationService)
        {
            this.worldStateService = worldStateService;
            this.vectorCalculatorService = vectorCalculatorService;
            engineConfig = configurationService.Value;
        }

        public bool IsApplicable(PlayerAction action) => action.Action == PlayerActions.FireTeleport;

        public void ProcessAction(BotObject bot)
        {
            if (bot.TeleporterCount <= 0)
            {
                return;
            }

            var position = vectorCalculatorService.GetPositionFrom(
                bot.Position,
                bot.Size + engineConfig.Teleport.Size + 1,
                bot.CurrentAction.Heading);

            worldStateService.AddTeleport(bot.CurrentAction.Heading, position, bot);
        }
    }
}