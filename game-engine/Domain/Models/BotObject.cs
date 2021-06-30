using System.Collections.Generic;

namespace Domain.Models
{
    public class BotObject : MovableGameObject
    {
        public List<PlayerAction> PendingActions { get; set; }
        public PlayerAction LastAction { get; set; }
        public PlayerAction CurrentAction { get; set; }
        public int Score { get; set; }
        public int Placement { get; set; }
        public int Seed { get; set; }
        public int TorpedoSalvoCount { get; set; }
        public int ShieldCount { get; set; }
        public int SupernovaAvailable { get; set; }

        public int TeleporterCount { get; set; }

        public new List<int> ToStateList() =>
            new List<int>
            {
                Size,
                Speed,
                CurrentHeading,
                (int) GameObjectType,
                Position.X,
                Position.Y,
                Effects.GetHashCode(),
                TorpedoSalvoCount,
                SupernovaAvailable,
                TeleporterCount,
                ShieldCount
            };
    }
}