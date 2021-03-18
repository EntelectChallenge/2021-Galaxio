using System;
using Domain.Models;

namespace Engine.Interfaces
{
    public interface IActionService
    {
        void PushPlayerAction(Guid botId, PlayerAction playerAction);
        void ApplyActionToBot(BotObject bot);
    }
}