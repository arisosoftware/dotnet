//using Serilog.Core;
//using Serilog.Events;
//using SerilogMask.Core.Logging;
//using System.Collections.Generic;
//using System.Linq;

//namespace SerilogMask.Core.Logging;

//public class SensitiveDataDestructuringPolicy : IDestructuringPolicy
//{


//    private readonly List<KeyValuePair<string, string>> _rules;

//    public SensitiveDataDestructuringPolicy(IEnumerable<MaskingConfig> rules)
//    {
//        // Convert each MaskingConfig into a KeyValuePair<string, string>
//        _rules = rules.Select(r => new KeyValuePair<string, string>(r.PropertyName, r.Mask)).ToList();
//    }

//    public bool TryDestructure(object value, ILogEventPropertyValueFactory factory, out LogEventPropertyValue result)
//    {
//        result = null;
//        if (value == null) return false;

//        var type = value.GetType();
//        var props = type.GetProperties();
//        var dict = new Dictionary<string, LogEventPropertyValue>();
//        // Apply rule-based masking for sensitive data
//        foreach (var prop in props)
//        {
//            var propName = prop.Name;
//            var propValue = prop.GetValue(value);
//            bool isSensitive = prop.GetCustomAttributes(typeof(SensitiveDataAttribute), true).Any();

//            // Check if the property name exists in _rules.Key and replace its value
//            var matchingRule = _rules.FirstOrDefault(rule => rule.Key == propName);
//            if (!string.IsNullOrEmpty(matchingRule.Key))
//            {
//                propValue = matchingRule.Value;
//            }

//            // If the property has the SensitiveDataAttribute, mark it as "***MASKED***"
//            if (isSensitive)
//            {
//                propValue = "***MASKED***";
//            }

//            dict[prop.Name] = new ScalarValue(propValue);
//        } 

//        result = new StructureValue(dict.Select(kv => new LogEventProperty(kv.Key, kv.Value)));
//        return true;
//    }

//    public bool TryDestructure2(object value, ILogEventPropertyValueFactory factory, out LogEventPropertyValue result)
//    {
//        result = null;
//        if (value == null) return false;

//        var type = value.GetType();
//        var props = type.GetProperties();
//        var dict = new Dictionary<string, LogEventPropertyValue>();

//        foreach (var prop in props)
//        {
//            var propValue = prop.GetValue(value);
//            bool isSensitive = prop.GetCustomAttributes(typeof(SensitiveDataAttribute), true).Any();

//            // Apply rule-based masking for sensitive data
//            if (propValue is string strValue)
//            {
//                foreach (var rule in _rules)
//                {
//                    if (strValue.Contains(rule.Key))
//                    {
//                        propValue = rule.Value;
//                        break;  // Stop after applying the first matching rule
//                    }
//                }
//            }

//            // If the value is a nested object, recursively apply masking
//            if (propValue != null && !isSensitive && !propValue.GetType().IsPrimitive && !(propValue is string))
//            {
//                propValue = MaskNestedObject(propValue);
//            }

//            dict[prop.Name] = new ScalarValue(isSensitive ? "***MASKED***" : propValue);
//        }

//        result = new StructureValue(dict.Select(kv => new LogEventProperty(kv.Key, kv.Value)));
//        return true;
//    }

//    private object MaskNestedObject(object nestedObject)
//    {
//        var type = nestedObject.GetType();
//        var props = type.GetProperties();
//        foreach (var prop in props)
//        {
//            var propValue = prop.GetValue(nestedObject);
//            if (propValue is string strValue)
//            {
//                foreach (var rule in _rules)
//                {
//                    if (strValue.Contains(rule.Key))
//                    {
//                        prop.SetValue(nestedObject, rule.Value);
//                        break;
//                    }
//                }
//            }
//            else if (propValue != null && !propValue.GetType().IsPrimitive && !(propValue is string))
//            {
//                // Recursively mask nested objects
//                prop.SetValue(nestedObject, MaskNestedObject(propValue));
//            }
//        }
//        return nestedObject;
//    }
//}

 