

using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;

namespace SerilogMask.Core.Logging
{
    public class SerilogMaskingEnricher : ILogEventEnricher
    {
        private readonly Dictionary<string, string> _directlyMaskDict;
        private readonly Dictionary<string, string> _structMaskDict;

        public Dictionary<string, string> GetDirectlyMaskDict()
        {
            return _directlyMaskDict;
        }


        public SerilogMaskingEnricher(Dictionary<string, string> directlyMaskDict, Dictionary<string, string> structMaskDict)
        {
            _directlyMaskDict = directlyMaskDict;
            _structMaskDict = structMaskDict;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            // Create a new list to hold updated properties
            var updatedProperties = new Dictionary<string, LogEventPropertyValue>();

            // Iterate over the log event properties
            foreach (var prop in logEvent.Properties)
            {
                string propName = prop.Key;
                LogEventPropertyValue propValue = prop.Value;
                string? maskValue = null;
                // Check if the property is a direct property (not part of a structure)
                if (_directlyMaskDict.TryGetValue(propName, out maskValue))
                {
                    if (string.IsNullOrEmpty(maskValue))
                    {
                        continue;
                    }
                    updatedProperties[propName] = new ScalarValue(maskValue);
                }
                else if (propValue is StructureValue structVal)
                {
                    var maskedProps = new List<LogEventProperty>();

                    foreach (var p in structVal.Properties)
                    {
                        string fullName = propName + "." + p.Name; // Full key (e.g., UserDTO.Email)

                        if (_structMaskDict.TryGetValue(fullName, out maskValue))
                        {
                            if (string.IsNullOrEmpty(maskValue))
                            {
                                continue;
                            }
                            maskedProps.Add(new LogEventProperty(p.Name, new ScalarValue(maskValue)));
                        }
                        else
                        {
                            maskedProps.Add(p);
                        }
                    }
                    StructureValue maskedStruct = new StructureValue(maskedProps, structVal.TypeTag);
                    updatedProperties[propName] = maskedStruct;
                }
                else
                {
                    updatedProperties[propName] = propValue;
                }
            }

            // Clear existing properties and add updated ones, is it required?

            foreach (var prop in updatedProperties)
            {
                logEvent.AddOrUpdateProperty(new LogEventProperty(prop.Key, prop.Value));
            }
        }
    }
}


//public class SerilogMaskingEnricher : ILogEventEnricher
//{
//    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
//    {
//        foreach (var property in logEvent.Properties)
//        {
//            if (property.Key.Contains("password", StringComparison.OrdinalIgnoreCase))
//            {
//                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(property.Key, "****"));
//            }
//        }
//    }
//}

//public class MaskingEnricher : ILogEventEnricher
//{
//    private readonly Dictionary<string, string> _maskMap;

//    public MaskingEnricher(Dictionary<string, string> maskMap) => _maskMap = maskMap;

//    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
//    {
//        foreach (var kvp in logEvent.Properties.ToList())
//        {
//            if (_maskMap.TryGetValue(kvp.Key, out var mask))
//            {
//                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(kvp.Key, mask));
//            }
//            else if (kvp.Value is StructureValue structVal && structVal.TypeTag == "UserDTO")
//            {
//                var maskedProps = structVal.Properties
//                    .Select(p =>
//                        _maskMap.TryGetValue("UserDTO." + p.Name, out var m)
//                            ? new LogEventProperty(p.Name, new ScalarValue(m))
//                            : p)
//                    .ToList();

//                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(kvp.Key, new StructureValue(maskedProps, structVal.TypeTag)));
//            }
//        }
//    }


//}