using LanDiscoveryServer.Models;
using LanDiscoveryServer.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<PeerManager>();
builder.Services.AddSingleton<UdpBroadcastService>();
builder.Services.AddSingleton<ActiveScannerService>();


var app = builder.Build();

// 启动广播服务
var broadcastService = app.Services.GetRequiredService<UdpBroadcastService>();
var lifetime = app.Lifetime;
var token = new CancellationTokenSource();
lifetime.ApplicationStopping.Register(() => token.Cancel());
await broadcastService.StartAsync(token.Token);

// API: 获取本机信息
app.MapGet("/api/self", (PeerManager peers) => peers.GetSelfInfo());

// API: 获取在线用户
app.MapGet("/api/peers", (PeerManager peers) => peers.GetPeers());
app.MapGet("/", () => "Hello World!");
// API: 更新在线用户信息
app.MapPost("/api/scan", async (ActiveScannerService scanner) =>
{
    await scanner.StartScanAsync();
    return Results.Ok("Scan started");
});

// 可选：测试 ping
app.MapGet("/api/ping/{ip}", async (string ip) =>
{
    using var ping = new System.Net.NetworkInformation.Ping();
    try
    {
        var reply = await ping.SendPingAsync(ip, 1000);
        return reply.Status.ToString();
    }
    catch
    {
        return "Error";
    }
});

var scanner = app.Services.GetRequiredService<ActiveScannerService>();
await scanner.StartScanAsync(); // 初始启动时主动扫描一次
app.UseDefaultFiles();
app.UseStaticFiles();
app.Run();