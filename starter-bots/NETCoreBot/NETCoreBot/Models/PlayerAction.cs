using NETCoreBot.Enums;
using System;

namespace NETCoreBot.Models
{
    public class PlayerAction
    {
        public Guid PlayerId { get; set; }
        public PlayerActions Action { get; set; }
        public int? Heading { get; set; }
    }
}
