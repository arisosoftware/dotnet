using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Parsing;
using SerilogMask.Core.Logging;
using System.Text.Json;

namespace SerilogMask.Tests;




public class SerilogHelperTest
{
    private readonly SerilogInMemorySink _sink;
    private readonly LoggerConfiguration _loggerConfiguration;
    private readonly ILogger _logger;



    public SerilogHelperTest()
    {
        // Load the masking configuration from appsettings.json

        // please convert json string to Dictionary ....
        Dictionary<string, string> mockcfg = new Dictionary<string, string>()
        {
            // let's create some mock data
            { "Email", "***Mask***" },
            { "UserDTO.Email", "***MaskEmail***" },
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
    public void Test_Info_Warning_Debug()
    {
        _logger.Information("Test message Info");
        _logger.Warning("Test message Warning");
        _logger.Debug("Test message Debug");
        // Assert
        var LogEvents = _sink.GetAllEvents();
        Assert.Equal(3, LogEvents.Count);
        Assert.Equal("Test message Info", LogEvents.ElementAt(0).MessageTemplate.Text);
        Assert.Equal("Test message Warning", LogEvents.ElementAt(1).MessageTemplate.Text);
        Assert.Equal("Test message Debug", LogEvents.ElementAt(2).MessageTemplate.Text);

    }

    [Fact]
    public void Test_Info_Warning_Debug_withMask()
    {
        _logger.Information("Test message Info {Email} ", "123456");
        _logger.Warning("Test message Warning {Email} ", "123456");
        _logger.Debug("Test message Debug {Email} ", "123456");
        // Assert
        var LogEvents = _sink.GetAllEvents();
        Assert.Equal(3, LogEvents.Count);
        Assert.Equal("Test message Info \"***Mask***\" ", LogEvents.ElementAt(0).RenderMessage());
        Assert.Equal("Test message Warning \"***Mask***\" ", LogEvents.ElementAt(1).RenderMessage());
        Assert.Equal("Test message Debug \"***Mask***\" ", LogEvents.ElementAt(2).RenderMessage());

    }

    [Fact]
    public void Test_Info_Warning_Debug_withMaskDto()
    {
        // Arrange
        UserDTO mockuser = new UserDTO()
        {
            // Create random user test sample here.
            Name = "John Doe",
            Email = "johndoe@example.com",
            Phone = "123-456-7890",
            Address = "123 Main St",
            City = "Sample City"
        };

        // Act
        _logger.Information("User details: {@UserDTO}", mockuser);

        // Assert
        var LogEvents = _sink.GetAllEvents();
        Assert.Single(LogEvents);
        string theLogLine = LogEvents.ElementAt(0).RenderMessage();
        
        Assert.Contains("***MaskName***", LogEvents.ElementAt(0).RenderMessage());
        Assert.Contains("***MaskEmail***", LogEvents.ElementAt(0).RenderMessage());
        Assert.Equal("User details: UserDTO { Name: \"***MaskName***\", Email: \"***MaskEmail***\", Phone: \"123-456-7890\", Address: \"123 Main St\", City: \"Sample City\" }", theLogLine);
    }

}
