using Domain.Models;

namespace Engine.Interfaces
{
    public interface ITickProcessingService
    {
        void SimulateTick(BotObject bot);
    }
}