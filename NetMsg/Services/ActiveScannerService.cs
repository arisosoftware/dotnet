using System.Net.NetworkInformation;
using LanDiscoveryServer.Models;

namespace LanDiscoveryServer.Services;

public class ActiveScannerService
{
    private readonly PeerManager _peerManager;

    public ActiveScannerService(PeerManager peerManager)
    {
        _peerManager = peerManager;
    }

    public async Task StartScanAsync(string subnet = "192.168.1")
    {
        var tasks = new List<Task>();
        for (int i = 1; i < 255; i++)
        {
            var ip = $"{subnet}.{i}";
            tasks.Add(Task.Run(() => PingAndRegister(ip)));
        }
        await Task.WhenAll(tasks);
    }

    private async Task PingAndRegister(string ip)
    {
        using var ping = new Ping();
        try
        {
            var reply = await ping.SendPingAsync(ip, 300);
            if (reply.Status == IPStatus.Success)
            {
                // 发出 HTTP 请求看对方是否在线提供 /api/self
                using var client = new HttpClient();
                var res = await client.GetAsync($"http://{ip}:5000/api/self");
                if (res.IsSuccessStatusCode)
                {
                    var json = await res.Content.ReadAsStringAsync();
                    var peer = System.Text.Json.JsonSerializer.Deserialize<PeerInfo>(json);
                    if (peer != null)
                        _peerManager.UpdatePeer(ip, peer.Name);
                }
            }
        }
        catch
        {
            // 忽略错误
        }
    }
}
