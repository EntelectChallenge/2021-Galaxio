using System;
using GameRunner.Enums;
using GameRunner.Interfaces;

namespace GameRunner.Services
{
    public class EnvironmentService : IEnvironmentService
    {
        private readonly string appEnvironment;

        public EnvironmentService()
        {
            appEnvironment = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "local";
            ApiUrl = IsCloud ? Environment.GetEnvironmentVariable("API_URL") : null;
            ApiKey = IsCloud ? Environment.GetEnvironmentVariable("API_KEY") : null;
            MatchId = IsCloud ? Environment.GetEnvironmentVariable("MATCH_ID") : null;
        }

        public string ApiUrl { get; }
        public string ApiKey { get; }
        public string MatchId { get; }
        public bool IsLocal => appEnvironment.Equals(Environments.Local, StringComparison.InvariantCultureIgnoreCase);
        public bool IsStaging => appEnvironment.Equals(Environments.Staging, StringComparison.InvariantCultureIgnoreCase);
        public bool IsProduction => appEnvironment.Equals(Environments.Production, StringComparison.InvariantCultureIgnoreCase);
        public bool IsCloud => !IsLocal;
    }
}