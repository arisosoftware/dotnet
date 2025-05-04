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

    private readonly Dictionary<string, string> mockcfg;
    private readonly string mockcfgjson;
    public SerilogHelperTest()
    {
                
        mockcfg = new Dictionary<string, string>()
        {
        // let's create some mock data
        { "Email", "***Mask***" },
        { "UserDTO.Email", "***MaskEmail***" },
        { "UserDTO.Name", "***MaskName***" }
        };

        mockcfgjson = JsonSerializer.Serialize<Dictionary<string, string>>(mockcfg);

    }

    [Fact]
    public void TestSerilogHelperJsonLoad()
    {
        SerilogMaskingEnricher maskenrich = SerilogHelper.LoadMaskingEnricherConfigByString(mockcfgjson);
        // Assert
        Assert.NotNull(maskenrich);
        Assert.Equal(3, maskenrich.GetDirectlyMaskDict().Count);
        Assert.Equal(3, maskenrich.GetStructMaskDict().Count);
        Assert.Equal("***Mask***", maskenrich.GetDirectlyMaskDict()["Email"]);
        Assert.Equal("***MaskEmail***", maskenrich.GetStructMaskDict()["UserDTO.Email"]);
        Assert.Equal("***MaskName***", maskenrich.GetStructMaskDict()["UserDTO.Name"]);


    }
     

}
