

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

        public Dictionary<string, string> GetStructMaskDict()
        {
            return _structMaskDict;
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


        public static SerilogMaskingEnricher LoadMaskingEnricherConfigByString(string json)
        {
            var config = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            return new SerilogMaskingEnricher(config["directlyMask"], config["structMask"]);

        }
        public static SerilogMaskingEnricher LoadMaskingEnricherConfigByDictionary(Dictionary<string, string> directlyMaskDict, Dictionary<string, string> structMaskDict)
        {
            if (directlyMaskDict == null || structMaskDict == null)
            {
                throw new ArgumentNullException(nameof(directlyMaskDict));
            }
            return new SerilogMaskingEnricher(directlyMaskDict, structMaskDict);
        }
        public static SerilogMaskingEnricher LoadMaskingEnricherConfigByDictionary(Dictionary<string, string> directlyMaskDict)
        {
            if (directlyMaskDict == null)
            {
                throw new ArgumentNullException(nameof(directlyMaskDict));
            }
            return new SerilogMaskingEnricher(directlyMaskDict, new Dictionary<string, string>());
        }
    }
}

