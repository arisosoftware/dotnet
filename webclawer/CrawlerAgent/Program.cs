
using Microsoft.EntityFrameworkCore;
using CrawlerShared.Db;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddDbContext<CrawlerDbContext>(options =>
    options.UseMySql("server=localhost;user=root;password=123456;database=crawlerdb;", ServerVersion.AutoDetect("server=localhost;user=root;password=123456;database=crawlerdb;")));
builder.Services.AddHttpClient();
builder.Services.AddHostedService<AgentWorker>();
var app = builder.Build();
app.MapControllers();
app.Run();
