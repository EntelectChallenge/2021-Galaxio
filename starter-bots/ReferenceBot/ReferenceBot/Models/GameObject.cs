using System;
using System.Collections.Generic;
using NETCoreBot.Enums;

namespace NETCoreBot.Models
{
    public class GameObject
    {
        public Guid Id { get; set; }
        public int Size { get; set; }
        public int Speed { get; set; }
        public ObjectTypes GameObjectType { get; set; }
        public int CurrentHeading { get; set; }
        public Position Position { get; set; }
        public int TorpedoSalvoCount { get; set; }

        public static GameObject FromStateList(Guid id, List<int> stateList)
        {
            if (stateList.Count == 8)
            {
                return new GameObject
                {
                    Id = id,
                    Size = stateList[0],
                    Speed = stateList[1],
                    CurrentHeading = stateList[2],
                    GameObjectType = (ObjectTypes) stateList[3],
                    Position = new Position {X = stateList[4], Y = stateList[5]},
                    TorpedoSalvoCount = stateList[7]
                };
            }
            else
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
}