
using Microsoft.Extensions.Hosting;
using CrawlerShared.Db;
using CrawlerShared.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using HtmlAgilityPack;

public class AgentWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IHttpClientFactory _httpClientFactory;

    public AgentWorker(IServiceProvider services, IHttpClientFactory httpClientFactory)
    {
        _services = services;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CrawlerDbContext>();
            var http = _httpClientFactory.CreateClient();

            var response = await http.GetAsync("http://localhost:5000/api/task/pending?agentId=agent001");
            if (!response.IsSuccessStatusCode) { await Task.Delay(3000); continue; }

            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json) || json == "null") { await Task.Delay(5000); continue; }

            var task = JsonSerializer.Deserialize<CrawlerTask>(json);
            if (task == null) continue;

            try
            {
                var param = JsonSerializer.Deserialize<Dictionary<string, string>>(task.Parameters);
                if (!param.TryGetValue("bbsid", out var bbsid)) continue;

                var url = $"https://web.6parkbbs.com/index.php?act=bbs&bbsid={bbsid}";
                var html = await http.GetStringAsync(url);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var nodes = doc.DocumentNode.SelectNodes("//div[@class='card-text']//a[contains(@href, 'tid=')]");
                if (nodes == null) continue;

                foreach (var node in nodes)
                {
                    var href = node.GetAttributeValue("href", "");
                    var title = node.InnerText.Trim();
                    var uri = new Uri("https://web.6parkbbs.com/" + href);
                    var tid = System.Web.HttpUtility.ParseQueryString(uri.Query).Get("tid");
                    if (int.TryParse(tid, out int tidVal))
                    {
                        db.PostContents.Add(new PostContent
                        {
                            Tid = tidVal,
                            Url = uri.ToString(),
                            Title = title,
                            BoardId = bbsid,
                            PostTime = DateTime.UtcNow
                        });
                    }
                }

                task.Status = "Completed";
                task.UpdateTime = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                task.Status = "Failed";
                task.UpdateTime = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }

            await Task.Delay(5000, stoppingToken);
        }
    }
}
