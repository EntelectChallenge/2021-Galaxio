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
            var torpedoSalvo = new TorpedoGameObject
            {
                Id = Guid.NewGuid(),
                Position = vectorCalculatorService.GetPositionFrom(
                    bot.Position,
                    bot.Size + engineConfig.Torpedo.Size + 1,
                    bot.CurrentAction.Heading),
                Size = engineConfig.Torpedo.Size,
                Speed = engineConfig.Torpedo.Speed,
                CurrentHeading = bot.CurrentAction.Heading,
                FiringPlayerId = bot.Id,
                IsMoving = true
            };
            bot.Size -= engineConfig.Torpedo.Cost;
            worldStateService.UpdateBotSpeed(bot);

            worldStateService.AddGameObject(torpedoSalvo);
        }
    }
}