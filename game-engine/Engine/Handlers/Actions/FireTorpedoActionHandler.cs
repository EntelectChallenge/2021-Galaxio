using System;
using Domain.Enums;
using Domain.Models;
using Engine.Handlers.Interfaces;
using Engine.Interfaces;
using Engine.Models;
using Engine.Services;

namespace Engine.Handlers.Actions
{
    public class FireTorpedoActionHandler : IActionHandler
    {
        private readonly IWorldStateService worldStateService;
        private readonly IVectorCalculatorService vectorCalculatorService;
        private readonly EngineConfig engineConfig;

        public FireTorpedoActionHandler(
            IWorldStateService worldStateService,
            IVectorCalculatorService vectorCalculatorService,
            IConfigurationService configurationService)
        {
            this.worldStateService = worldStateService;
            this.vectorCalculatorService = vectorCalculatorService;
            engineConfig = configurationService.Value;
        }

        public bool IsApplicable(PlayerAction action) => action.Action == PlayerActions.FireTorpedoes;

        public void ProcessAction(BotObject bot)
        {
            if (bot.TorpedoSalvoCount < 1)
            {
                return;
            }

            worldStateService.AddTorpedo(bot);
        }
    }
}