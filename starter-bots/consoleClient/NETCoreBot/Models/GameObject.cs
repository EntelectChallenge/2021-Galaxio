using NETCoreBot.Enums;
using System;

namespace NETCoreBot.Models
{
    public class GameObject
    {
        public Guid Id { get; set; }
        public int Size { get; set; }
        public Position Position { get; set; }
        public ObjectTypes ObjectType { get; set; }
    }
}
