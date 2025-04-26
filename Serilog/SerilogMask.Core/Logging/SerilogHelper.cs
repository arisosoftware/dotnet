using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace SerilogMask.Core.Logging;

public static class SerilogHelper
{
    public static void SetupSerilog()
    {
 
        // please convert json string to Dictionary ....
        Dictionary<string, string> mockcfg = new Dictionary<string, string>()
        {
            // let's create some mock data
            { "Email", "***Mask***" },
            { "UserDTO.Email", "***Mask1***" },
            { "UserDTO.Name", "***MaskName***" }
        };

        string mockcfgjson = JsonSerializer.Serialize<Dictionary<string, string>>(mockcfg);

        var enricher = LoadMaskingEnricherConfigByString(mockcfgjson);

        // var sensitivePolicy = new SensitiveDataDestructuringPolicy();

        Log.Logger = new LoggerConfiguration()
           .Enrich.With(enricher)
            //    .Destructure.With(sensitivePolicy)  // Use the instance, not the generic type
            .WriteTo.Console()

           // .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }
    public static SerilogMaskingEnricher LoadMaskingEnricherConfig(string configFilePath)
    {
        // Read the config file
        var json = File.ReadAllText(configFilePath);
        return LoadMaskingEnricherConfigByString(json);
    }
    // Existing code remains unchanged
    public static SerilogMaskingEnricher LoadMaskingEnricherConfigByString(string json)
    {
        var configDict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

        if (configDict == null)
        {
            throw new ArgumentNullException(nameof(json), "The provided JSON string could not be deserialized into a dictionary.");
        }

        // Separate into Direct and Struct masks
        var directlyMaskDict = new Dictionary<string, string>();
        var structMaskDict = new Dictionary<string, string>();

        foreach (var entry in configDict)
        {
            string key = entry.Key;
            string maskValue = entry.Value;

            if (key.Contains("."))
            {
                // It's a struct property (e.g., "UserDTO.Email")
                structMaskDict[key] = maskValue;
            }
            else
            {
                // It's a direct property (e.g., "Email")
                directlyMaskDict[key] = maskValue;
            }
        }

        return new SerilogMaskingEnricher(directlyMaskDict, structMaskDict);
    }
}
 