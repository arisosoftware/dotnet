using FakeMail.Web.Components;
using FakeMail.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddServerSideBlazor();
builder.Services.AddRazorPages(); // Add Razor Pages services

builder.Services.AddDbContext<FakeMailDbContext>(options =>
    options.UseSqlite("Data Source=../FakeMail.Server/fakemail.db"));

var app = builder.Build();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host"); // Ensure Razor Pages fallback works

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();


