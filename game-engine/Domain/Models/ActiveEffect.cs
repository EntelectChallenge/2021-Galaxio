using Domain.Enums;

namespace Domain.Models
{
    public class ActiveEffect
    {
        public MovableGameObject Bot { get; set; }
        public Effects Effect { get; set; }
        public int EffectDuration { get; set; }
    }
}