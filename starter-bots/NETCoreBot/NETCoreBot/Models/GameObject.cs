using NETCoreBot.Enums;
using System;
using System.Collections.Generic;

namespace NETCoreBot.Models
{
    public class GameObject
    {
        public Guid Id { get; set; }
        public int Size { get; set; }
        public int Speed { get; set; }
        public Position Position { get; set; }
        public int CurrentHeading { get; set; }
        public ObjectTypes GameObjectType { get; set; }
        
        public static GameObject FromStateList(Guid id, List<int> stateList)
        {
            return new GameObject
            {
                Id = id,
                Size = stateList[0],
                Speed = stateList[1],
                CurrentHeading = stateList[2],
                GameObjectType = (ObjectTypes) stateList[3],
                Position = new Position {X = stateList[4], Y = stateList[5]}
            };
        }
    }
}
