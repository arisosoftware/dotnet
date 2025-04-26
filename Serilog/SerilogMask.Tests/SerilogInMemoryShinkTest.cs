using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Parsing;
using SerilogMask.Core.Logging;
using System.Text.Json;

namespace SerilogMask.Tests;

public class SerilogInMemoryShinkTest
{
    private readonly SerilogInMemorySink _sink;
    private readonly LoggerConfiguration _loggerConfiguration;
    private readonly ILogger _logger;
    
    public SerilogInMemoryShinkTest()
    {
        // Load the masking configuration from appsettings.json

        // please convert json string to Dictionary ....
        Dictionary<string, string> mockcfg = new Dictionary<string, string>()
        {
            // let's create some mock data
            { "Email", "***Mask***" },
            { "UserDTO.Email", "***Mask1***" },
            { "UserDTO.Name", "***MaskName***" }
        };

        string mockcfgjson = JsonSerializer.Serialize<Dictionary<string, string>>(mockcfg);
        
        var mask = SerilogHelper.LoadMaskingEnricherConfigByString(mockcfgjson);
         
        _sink = new SerilogInMemorySink();
        _loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.With(mask)  
            .WriteTo.Sink(_sink);
        _logger = _loggerConfiguration.CreateLogger();
    }


    [Fact]
    public void GiveAnEvent_WhenLogEventIsEmitted_ThenItShouldBeStoredInSink()
    {
        // Arrange
        var logEvent = new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            null,
            new MessageTemplate("Test message", Enumerable.Empty<MessageTemplateToken>()),
            new List<LogEventProperty>()
        );
        // Act
        _sink.Emit(logEvent);
        // Assert
        var LogEvents = _sink.GetAllEvents();
        Assert.Single(LogEvents);
        Assert.Equal(logEvent, LogEvents.First());
    }

    [Fact]
    public void GivenAJsonConfig_WhenMaskingEnricherIsLoaded_ThenItShouldContainMaskedProperties()
    {
        // Arrange
        var config = new Dictionary<string, string>
        {
            { "Email", "****" },
            { "UserDTO.Email", "****" }
        };
        var maskingEnricher = new SerilogMaskingEnricher(config, config);
        // Act
        _loggerConfiguration.Enrich.With(maskingEnricher);
        // Assert
        Assert.NotNull(maskingEnricher);
    }

    [Fact]
    public void Test_Event1_2_inorder()
    {
        // Arrange
        var logEvent1 = new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            null,
            new MessageTemplate("Test message 1", Enumerable.Empty<MessageTemplateToken>()),
            new List<LogEventProperty>()
        );
        var logEvent2 = new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            null,
            new MessageTemplate("Test message 2", Enumerable.Empty<MessageTemplateToken>()),
            new List<LogEventProperty>()
        );
        // Act
        _sink.Emit(logEvent1);
        _sink.Emit(logEvent2);
        // Assert
        var LogEvents = _sink.GetAllEvents();
        Assert.Equal(2, LogEvents.Count);
        Assert.Equal(logEvent1, LogEvents.ElementAt(0));
        Assert.Equal(logEvent2, LogEvents.ElementAt(1));


    }
}
