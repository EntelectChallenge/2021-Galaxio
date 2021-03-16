using System;

namespace NETCoreBot.Models
{
    public class PlayerAction
    {
        public Guid PlayerId { get; set; }
        public int ActionId { get; set; }
        public int? Heading { get; set; }
    }
}
