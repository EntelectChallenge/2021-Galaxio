using System.Collections.Generic;
using Domain.Models;

namespace Engine.Models
{
    public class BotPath
    {
        public BotObject Bot { get; set; }
        public Position MovementEndpoint { get; set; }
        public List<Position> CollisionDetectionPoints { get; set; }
        public bool Moved { get; set; }
        public bool HasCollided { get; set; }
        public Position MovementStartPoint { get; set; }
    }
}