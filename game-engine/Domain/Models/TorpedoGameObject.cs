using System;
using Domain.Enums;

namespace Domain.Models
{
    public class TorpedoGameObject : MovableGameObject
    {
        public TorpedoGameObject()
        {
            GameObjectType = GameObjectType.TorpedoSalvo;
        }

        public Guid FiringPlayerId { get; set; }
    }
}