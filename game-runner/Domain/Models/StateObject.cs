using Microsoft.AspNetCore.SignalR;

namespace Domain.Models
{
    public class StateObject
    {
        public string ConnectionId { get; set; }
        public IClientProxy Client { get; set; }
        public int PreviousTick { get; set; }
        public int CurrentTick { get; set; }
    }
}