using GameRunner.Enums;
using GameRunner.Models;

namespace GameRunner.Interfaces
{
    public interface ICloudCallbackFactory
    {
        CloudCallback Make(CloudCallbackType cloudCallbackType);
    }
}