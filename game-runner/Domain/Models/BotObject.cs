using System.Collections.Generic;

namespace Domain.Models
{
    public class BotObject : GameObject
    {
        public List<PlayerAction> PendingActions { get; set; }
        public PlayerAction LastAction { get; set; }
        public PlayerAction CurrentAction { get; set; }
    }
}