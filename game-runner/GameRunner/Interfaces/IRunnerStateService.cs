using System;
using System.Collections.Generic;
using Domain.Models;
using Microsoft.AspNetCore.SignalR;

namespace GameRunner.Interfaces
{
    public interface IRunnerStateService
    {
        Guid RegisterClient(string connectionId, string nickName, IClientProxy client);
        void DeregisterBot(string contextConnectionId);
        int TotalConnectedClients { get; }
        int TotalConnectedBots { get; }
        int TotalConnections { get; }
        string FailureReason { get; set; }
        bool IsCoreReady { get; }
        GameCompletePayload GameCompletePayload { get; set; }
        Guid? GetBotGuidFromConnectionId(string contextConnectionId);
        void StartGame();
        StateObject GetEngine();
        StateObject GetLogger();
        void RegisterEngine(string connectionId, IClientProxy client);
        void RegisterLogger(string connectionId, IClientProxy client);
        Dictionary<Guid, string> GetActiveConnections();
        void AddRegistrationToken(string connectionId, Guid token);
        string GetRegistrationToken(string botId);
        void StopApplication();
        Dictionary<Guid, string> GetRegistrationTokens();
        bool GetBotActionReceived(Guid id);
        void AddBotActionReceived(Guid id);
        void ClearBotActionsReceived();
    }
}