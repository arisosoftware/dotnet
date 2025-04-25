using Microsoft.Extensions.Configuration;
using Serilog;
using SerilogMask.Core.Logging;
using Serilog.Sinks.InMemory;
using Serilog.Sinks.InMemory.Assertions;
namespace SerilogMask.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {

    }

    [Fact]
    public void Should_Mask_Password_in_Log()
    {
        // Arrange
        var log = new LoggerConfiguration()
            .Enrich.With<SerilogMaskingEnricher>()
            .WriteTo.Console()
            .CreateLogger();

        // Act
        log.Information("User login with password: {Password}", "secretpassword");

        // Assert
        var lastLog = log.ToString();
        Assert.Contains("****", lastLog); // Masked password should appear as ****
    }


    public static MaskingConfig LoadMaskingConfig(string configPath = "appsettings.json")
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile(configPath, optional: false, reloadOnChange: true)
            .Build();

        return config.GetSection("Masking").Get<MaskingConfig>();
    }

    [Fact]
    public void ShouldLoadMockMaskingConfig()
    {
        var inMemorySettings = new Dictionary<string, string> {
        {"Masking:Should_Mask_Scalar_Email", "true"},
        {"Masking:Should_Mask_Scalar_SSN", "false"}
    };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var maskingConfig = configuration.GetSection("Masking").Get<MaskingConfig>();

        Assert.True(maskingConfig.Should_Mask_Scalar_Email);
        Assert.False(maskingConfig.Should_Mask_Scalar_SSN);
    }
    [Fact]
    public void Should_Mask_Scalar_Email()
    {
        var rules = new Dictionary<string, string> { { "Email", "***Mask***" } };

        var logger = new LoggerConfiguration()
            .Enrich.With(new MaskingEnricher(rules))
            .WriteTo.InMemory()  // From Serilog.Sinks.InMemory
            .CreateLogger();

        logger.Information("Testing {Email}", "secret@email.com");

        var log = InMemorySink.Instance.LogEvents.First();
        Assert.Contains("***Mask***", log.RenderMessage());
    }
}
