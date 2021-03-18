using Domain.Models;

namespace Engine.Handlers.Interfaces
{
    public interface IActionHandler
    {
        bool IsApplicable(PlayerAction botCurrentAction);
        void ProcessAction(BotObject bot);
    }
}