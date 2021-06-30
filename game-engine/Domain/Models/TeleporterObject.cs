using System;

namespace Domain.Models
{
    public class TeleporterObject : GameObject
    {
        public Guid FiringPlayerId { get; set; }
    }
}