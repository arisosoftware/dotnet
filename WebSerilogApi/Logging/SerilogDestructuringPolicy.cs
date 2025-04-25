using Serilog.Core;
using Serilog.Events;
using System.Collections.Generic;
using System.Linq;

public class SensitiveDataDestructuringPolicy : IDestructuringPolicy
{

    public SensitiveDataDestructuringPolicy()
    {
        // Default constructor
    }
    private readonly List<ISensitiveDataRule> _rules;

    public SensitiveDataDestructuringPolicy(IEnumerable<ISensitiveDataRule> rules)
    {
        _rules = rules.ToList();
    }

public bool TryDestructure(object value, ILogEventPropertyValueFactory factory, out LogEventPropertyValue result)
{
    result = null;

    if (value == null) return false;

    var type = value.GetType();
    var props = type.GetProperties();
    var dict = new Dictionary<string, LogEventPropertyValue>();

    foreach (var prop in props)
    {
        var propValue = prop.GetValue(value);
        bool isSensitive = prop.GetCustomAttributes(typeof(SensitiveDataAttribute), true).Any();

        // Apply rule-based masking for sensitive data
        if (propValue is string strValue)
        {
            foreach (var rule in _rules)
            {
                if (rule.IsSensitive(strValue))
                {
                    propValue = rule.MaskData(strValue);
                    break;
                }
            }
        }

        // Recursively mask nested objects
        if (propValue != null && !isSensitive && !propValue.GetType().IsPrimitive && !(propValue is string))
        {
            propValue = MaskNestedObject(propValue);
        }

        dict[prop.Name] = new ScalarValue(isSensitive ? "***MASKED***" : propValue);
    }

    result = new StructureValue(dict.Select(kv => new LogEventProperty(kv.Key, kv.Value)));
    return true;
}


    public bool TryDestructure2(object value, ILogEventPropertyValueFactory factory, out LogEventPropertyValue result)
    {
        result = null;

        if (value == null) return false;

        var type = value.GetType();
        var props = type.GetProperties();

        var dict = new Dictionary<string, LogEventPropertyValue>();

        foreach (var prop in props)
        {
            var propValue = prop.GetValue(value);
            bool isSensitive = prop.GetCustomAttributes(typeof(SensitiveDataAttribute), true).Any();

            // Apply rule-based masking for sensitive data
            if (propValue is string strValue)
            {
                foreach (var rule in _rules)
                {
                    if (rule.IsSensitive(strValue))
                    {
                        propValue = rule.MaskData(strValue);
                        break;  // Stop after applying the first matching rule
                    }
                }
            }

            // If the value is a nested object, recursively apply masking
            if (propValue != null && !isSensitive && !propValue.GetType().IsPrimitive && !(propValue is string))
            {
                propValue = MaskNestedObject(propValue);
            }

            dict[prop.Name] = new ScalarValue(isSensitive ? "***MASKED***" : propValue);
        }

        result = new StructureValue(dict.Select(kv => new LogEventProperty(kv.Key, kv.Value)));
        return true;
    }

    private object MaskNestedObject(object nestedObject)
    {
        var type = nestedObject.GetType();
        var props = type.GetProperties();
        foreach (var prop in props)
        {
            var propValue = prop.GetValue(nestedObject);
            if (propValue is string strValue)
            {
                foreach (var rule in _rules)
                {
                    if (rule.IsSensitive(strValue)) 
                    {
                        prop.SetValue(nestedObject, rule.MaskData(strValue));
                        break;
                    }
                }
            }
            else if (propValue != null && !propValue.GetType().IsPrimitive && !(propValue is string))
            {
                // Recursively mask nested objects
                prop.SetValue(nestedObject, MaskNestedObject(propValue));
            }
        }
        return nestedObject;
    }
}

 