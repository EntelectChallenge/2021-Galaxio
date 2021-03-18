using System;
using Domain.Enums;

namespace Domain.Models
{
    public class PlayerAction
    {
        public Guid? PlayerId { get; set; }
        public PlayerActions Action { get; set; }
        public int Heading { get; set; }
    }
}