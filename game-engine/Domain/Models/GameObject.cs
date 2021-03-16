using System;
using System.Collections.Generic;
using Domain.Enums;

namespace Domain.Models
{
    public class GameObject
    {
        public Guid Id { get; set; }
        public int Size { get; set; }
        public int Speed { get; set; }
        public GameObjectType GameObjectType { get; set; }
        public int CurrentHeading { get; set; }
        public Position Position { get; set; }
        public Effects Effects { get; set; }

        public List<int> ToStateList() =>
            new List<int>
            {
                Size,
                Speed,
                CurrentHeading,
                (int) GameObjectType,
                Position.X,
                Position.Y,
                Effects.GetHashCode()
            };

        public static GameObject FromStateList(Guid id, List<int> stateList) =>
            new GameObject
            {
                Id = id,
                Size = stateList[0],
                Speed = stateList[1],
                CurrentHeading = stateList[2],
                GameObjectType = (GameObjectType) stateList[3],
                Position = new Position
                {
                    X = stateList[4],
                    Y = stateList[5]
                },
                Effects = Enum.Parse<Effects>(stateList[6].ToString())
            };
    }
}