using GameRunner.Models;

namespace GameRunner.Interfaces
{
    public interface IConfigurationService
    {
        public RunnerConfig RunnerConfig { get; set; }
    }
}