using Domain.Enums;

namespace Domain.Models
{
    public class ActiveEffect
    {
        public BotObject Bot { get; set; }
        public Effects Effect { get; set; }
    }
}