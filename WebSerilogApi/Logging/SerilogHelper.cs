using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


public static class SerilogHelper
{
    public static void ConfigureLogger()
    {

        
        var sensitivePolicy = new SensitiveDataDestructuringPolicy(rules);

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Destructure.With(sensitivePolicy)  // Use the instance, not the generic type
            .WriteTo.Console()
            .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }
}
//     //     Log.Logger = new LoggerConfiguration()
//     //         .Enrich.FromLogContext()
//     //         .Destructure.With<SensitiveDataDestructuringPolicy>()
//     //         .WriteTo.Console()
//     //         .CreateLogger();
// }
// }