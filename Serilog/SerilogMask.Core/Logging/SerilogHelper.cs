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
        var enricher = LoadMaskingEnricherConfig("path/to/config.json");

        // var sensitivePolicy = new SensitiveDataDestructuringPolicy();

        Log.Logger = new LoggerConfiguration()
           .Enrich.With(enricher)
            //    .Destructure.With(sensitivePolicy)  // Use the instance, not the generic type
            .WriteTo.Console()
            .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }

    // Existing code remains unchanged
    public static MaskingEnricher LoadMaskingEnricherConfig(string configFilePath)
    {
        // Read the config file
        var json = File.ReadAllText(configFilePath);

        // Deserialize the JSON into a dictionary using System.Text.Json
        var configDict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

        // Separate into Direct and Struct masks
        var directlyMaskDict = new Dictionary<string, string>();
        var structMaskDict = new Dictionary<string, string>();

        foreach (var entry in configDict)
        {
            string key = entry.Key;
            string maskValue = entry.Value;

            // Determine if it's a direct property or struct property
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

        // Return the configured MaskingEnricher
        return new MaskingEnricher(directlyMaskDict, structMaskDict);
    }
}
 