using System.Threading.Tasks;
using GameRunner.Enums;

namespace GameRunner.Interfaces
{
    public interface ICloudIntegrationService
    {
        Task Announce(CloudCallbackType callbackType);
    }
}