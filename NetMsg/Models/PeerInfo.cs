namespace LanDiscoveryServer.Models;

public class PeerInfo
{
    public string Name { get; set; } = Environment.MachineName;
    public string IP { get; set; } = "";
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
}