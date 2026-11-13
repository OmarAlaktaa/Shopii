using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Serilog.Core;
using Serilog.Events;
using Shopii.Application.Common.Auditing;

namespace Shopii.API.Logging;

/// <summary>
/// Defense-in-depth redaction for objects that are accidentally destructured
/// with {@Object}. Scalar values and exceptions are left to Serilog.
/// </summary>
public sealed class SensitiveDataDestructuringPolicy : IDestructuringPolicy
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache =
        new();

    public bool TryDestructure(
        object value,
        ILogEventPropertyValueFactory propertyValueFactory,
        [NotNullWhen(true)] out LogEventPropertyValue? result)
    {
        result = null;

        var type = value.GetType();

        if (value is string
            || value is Exception
            || value is Type
            || value is Delegate
            || type.IsPrimitive
            || type.IsEnum
            || value is DateTime
            || value is DateTimeOffset
            || value is decimal
            || value is Guid)
        {
            return false;
        }

        if (value is IDictionary dictionary)
        {
            var elements = new List<KeyValuePair<ScalarValue, LogEventPropertyValue>>(
                dictionary.Count);

            foreach (DictionaryEntry entry in dictionary)
            {
                var key = entry.Key?.ToString() ?? string.Empty;
                var keyValue = new ScalarValue(key);

                elements.Add(
                    SensitiveNames.IsSensitive(key)
                        ? new KeyValuePair<ScalarValue, LogEventPropertyValue>(
                            keyValue,
                            new ScalarValue("[REDACTED]"))
                        : new KeyValuePair<ScalarValue, LogEventPropertyValue>(
                            keyValue,
                            propertyValueFactory.CreatePropertyValue(
                                entry.Value,
                                destructureObjects: true)));
            }

            result = new DictionaryValue(elements);
            return true;
        }

        if (value is IEnumerable)
        {
            return false;
        }

        var properties = PropertyCache.GetOrAdd(
            type,
            static objectType => objectType.GetProperties(
                BindingFlags.Instance | BindingFlags.Public));

        if (properties.Length == 0)
        {
            return false;
        }

        var logEventProperties = new List<LogEventProperty>(properties.Length);

        foreach (var property in properties)
        {
            if (property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            object? propertyValue;

            try
            {
                propertyValue = property.GetValue(value);
            }
            catch
            {
                // A getter that throws must not break logging.
                continue;
            }

            if (SensitiveNames.IsSensitive(property.Name))
            {
                logEventProperties.Add(
                    new LogEventProperty(
                        property.Name,
                        new ScalarValue("[REDACTED]")));
                continue;
            }

            logEventProperties.Add(
                new LogEventProperty(
                    property.Name,
                    propertyValueFactory.CreatePropertyValue(
                        propertyValue,
                        destructureObjects: true)));
        }

        result = new StructureValue(logEventProperties, type.Name);
        return true;
    }
}
