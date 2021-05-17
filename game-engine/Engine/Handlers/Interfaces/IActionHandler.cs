using Domain.Models;
using Engine.Services;

namespace Engine.Handlers.Interfaces
{
    public interface IActionHandler
    {
        bool IsApplicable(PlayerAction action);
        void ProcessAction(BotObject bot);
    }
}