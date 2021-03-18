namespace GameRunner.Interfaces
{
    public interface IEnvironmentService
    {
        public bool IsLocal { get; }
        public bool IsStaging { get; }
        public bool IsProduction { get; }
        public bool IsCloud { get; }
        public string ApiUrl { get; }
        public string ApiKey { get; }
        public string MatchId { get; }
    }
}