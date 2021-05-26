using Domain.Models;

namespace Engine.Handlers.Interfaces
{
    public interface IActionHandler
    {
        bool IsApplicable(PlayerAction action);
        void ProcessAction(BotObject bot);
    }
}