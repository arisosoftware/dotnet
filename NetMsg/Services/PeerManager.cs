using LanDiscoveryServer.Models;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets; 


namespace LanDiscoveryServer.Services;

public class PeerManager
{
    private readonly ConcurrentDictionary<string, PeerInfo> _peers = new();

    public void UpdatePeer(string ip, string name)
    {
        _peers[ip] = new PeerInfo
        {
            IP = ip,
            Name = name,
            LastSeen = DateTime.UtcNow
        };
    }

    public List<PeerInfo> GetPeers()
    {
        var now = DateTime.UtcNow;
        return _peers.Values
            .Where(p => (now - p.LastSeen).TotalSeconds < 120)
            .OrderBy(p => p.IP)
            .ToList();
    }

    public PeerInfo? GetSelfInfo()
    {
        var ip = GetLocalIPAddress();
        return new PeerInfo { IP = ip, Name = Environment.MachineName };
    }

    public static string GetLocalIPAddress()
    {
        return Dns.GetHostEntry(Dns.GetHostName())
                  .AddressList
                  .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)?
                  .ToString() ?? "127.0.0.1";
    }
    
}
