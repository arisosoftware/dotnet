using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LanDiscoveryServer.Services;

public class UdpBroadcastService
{
    private readonly PeerManager _peerManager;
    private readonly UdpClient _udpClient;
    private const int Port = 2425;

    public UdpBroadcastService(PeerManager peerManager)
    {
        _peerManager = peerManager;
        _udpClient = new UdpClient(Port);
        _udpClient.EnableBroadcast = true;
    }

    public async Task StartAsync(CancellationToken token)
    {
        // 广播自己上线
        _ = Task.Run(() => BroadcastLoop(token));
        // 监听其他设备广播
        _ = Task.Run(() => ReceiveLoop(token));
    }

    private async Task BroadcastLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var ip = PeerManager.GetLocalIPAddress();
            var msg = $"{Environment.MachineName}|{ip}";
            var data = Encoding.UTF8.GetBytes(msg);
            await _udpClient.SendAsync(data, data.Length, new IPEndPoint(IPAddress.Broadcast, Port));
            await Task.Delay(10000, token); // 每 10 秒广播一次
        }
    }

    private async Task ReceiveLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var result = await _udpClient.ReceiveAsync();
            var message = Encoding.UTF8.GetString(result.Buffer);
            var parts = message.Split('|');
            if (parts.Length == 2 && result.RemoteEndPoint.Address.ToString() != PeerManager.GetLocalIPAddress())
            {
                _peerManager.UpdatePeer(parts[1], parts[0]);
            }
        }
    }
}
