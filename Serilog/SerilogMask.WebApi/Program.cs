using Serilog;
using SerilogMask.Core.Logging;
using SerilogMask.WebApi;

var builder = WebApplication.CreateBuilder(args);



// Load the configuration from appsettings.json
var configuration = builder.Configuration;

// ---- STEP 1: Setup Serilog using SerilogHelper ----
SerilogHelper.SetupSerilog(configuration);  // Pass the configuration to the setup method



//// ---- STEP 1: Load appsettings.json ----
//builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

//// ---- STEP 2: Setup Serilog with MaskingEnricher ----
//Log.Logger = new LoggerConfiguration()
//    .ReadFrom.Configuration(builder.Configuration)
//    .Enrich.With(new SerilogMaskingEnricher(builder.Configuration))
//    .CreateLogger();

// Replace default logger
builder.Host.UseSerilog();

// ---- STEP 3: Add services ----
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


// ---- STEP 4: Run Serilog test ----
SerilogTester.RunTests();

// ---- STEP 5: Start app with global exception handling ----
try
{
    Log.Information("Starting up the application...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed!");
}
finally
{
    Log.CloseAndFlush();
}


app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}


 