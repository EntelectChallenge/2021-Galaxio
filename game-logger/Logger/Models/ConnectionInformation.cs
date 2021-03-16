using Logger.Enums;

namespace Logger.Models
{
    public class ConnectionInformation
    {
        public ConnectionStatus Status { get; set; }
        public string Reason { get; set; }
    }
}