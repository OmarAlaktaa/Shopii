using System.Globalization;

namespace Shopii.Application.Common.Auditing;

/// <summary>
/// Safe, allowlisted details for an audit event. Sensitive keys are redacted
/// and only scalar values are stored so callers cannot accidentally persist
/// request bodies, entity graphs, or PII.
/// </summary>
public sealed class AuditDetails
{
    public const int MaxKeys = 10;
    public const int MaxKeyLength = 64;
    public const int MaxStringLength = 200;

    private readonly Dictionary<string, object?> _values =
        new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, object?> Values => _values;

    public static AuditDetails Create(
        params (string Key, object? Value)[] values)
    {
        var details = new AuditDetails();

        foreach (var (key, value) in values)
        {
            details.Add(key, value);
        }

        return details;
    }

    public AuditDetails Add(string key, object? value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException(
                "Audit detail keys must not be empty.",
                nameof(key));
        }

        var isSensitive = SensitiveNames.IsSensitive(key);
        var safeKey = key.Length > MaxKeyLength
            ? key[..MaxKeyLength]
            : key;

        if (_values.Count >= MaxKeys && !_values.ContainsKey(safeKey))
        {
            throw new InvalidOperationException(
                $"Audit details support at most {MaxKeys} keys.");
        }

        _values[safeKey] = isSensitive
            ? "[REDACTED]"
            : Normalize(value);

        return this;
    }

    private static object? Normalize(object? value)
    {
        if (value is null)
        {
            return null;
        }

        if (value is string text)
        {
            return text.Length > MaxStringLength
                ? text[..MaxStringLength] + "..."
                : text;
        }

        if (value is Guid guid)
        {
            return guid.ToString();
        }

        if (value is DateTime dateTime)
        {
            return dateTime.ToUniversalTime();
        }

        if (value is DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.ToUniversalTime();
        }

        if (value is DateOnly dateOnly)
        {
            return dateOnly.ToString("O", CultureInfo.InvariantCulture);
        }

        if (value is TimeOnly timeOnly)
        {
            return timeOnly.ToString("O", CultureInfo.InvariantCulture);
        }

        if (value is Enum enumValue)
        {
            return enumValue.ToString();
        }

        return Type.GetTypeCode(value.GetType()) switch
        {
            TypeCode.Boolean or
            TypeCode.Byte or
            TypeCode.SByte or
            TypeCode.Int16 or
            TypeCode.UInt16 or
            TypeCode.Int32 or
            TypeCode.UInt32 or
            TypeCode.Int64 or
            TypeCode.UInt64 or
            TypeCode.Single or
            TypeCode.Double or
            TypeCode.Decimal => value,
            _ => "[UNSUPPORTED]",
        };
    }
}
