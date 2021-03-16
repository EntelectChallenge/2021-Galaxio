using GameRunner.Enums;

namespace GameRunner.Models
{
    public class ConnectionInformation
    {
        public ConnectionStatus Status { get; set; }
        public string Reason { get; set; }
    }
}