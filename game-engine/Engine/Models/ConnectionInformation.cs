using Engine.Enums;
using Engine.Services;

namespace Engine.Models
{
    public class ConnectionInformation
    {
        public ConnectionStatus Status { get; set; }
        public string Reason { get; set; }
    }
}