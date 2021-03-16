﻿using Domain.Enums;
using Domain.Models;
using Engine.Handlers.Interfaces;

namespace Engine.Handlers.Actions
{
    public class StopActionHandler : IActionHandler
    {
        public bool IsApplicable(PlayerAction botCurrentAction) => botCurrentAction.Action == PlayerActions.Stop;

        public void ProcessAction(BotObject bot)
        {
            bot.IsMoving = false;
        }
    }
}