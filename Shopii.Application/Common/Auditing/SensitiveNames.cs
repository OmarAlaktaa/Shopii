namespace Shopii.Application.Common.Auditing;

/// <summary>
/// Shared list of property names that must never be written to logs or audit
/// details. Keeping the list in the application layer lets both Serilog
/// destructuring and the audit writer use the same definition.
/// </summary>
public static class SensitiveNames
{
    private static readonly HashSet<string> Names =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Password",
            "PasswordHash",
            "PasswordConfirmation",
            "Salt",
            "Token",
            "AccessToken",
            "RefreshToken",
            "Authorization",
            "AuthorizationHeader",
            "ApiKey",
            "Secret",
            "ClientSecret",
            "PrivateKey",
            "ConnectionString",
            "Email",
            "EmailAddress",
            "Phone",
            "PhoneNumber",
            "Address",
            "Ssn",
            "DateOfBirth",
            "Dob",
            "IpAddress",
            "Ip",
            "CardNumber",
            "CardExpiry",
            "Expiry",
            "Cvv",
            "Cvc",
            "SecurityCode",
            "Pin",
            "SessionId",
            "Headers",
            "RequestHeaders",
            "ResponseHeaders",
            "Cookies",
            "Cookie",
            "SetCookie",
            "Body",
            "RequestBody",
            "QueryString",
            "Query",
            "RequestQuery",
            "Form",
            "RequestForm",
        };

    // These suffixes mark a boolean or timestamp about a sensitive field, not
    // the field's value. For example, PasswordChanged is safe to audit while
    // UserPassword is not.
    private static readonly HashSet<string> SafeSuffixes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Changed",
            "Updated",
            "Verified",
            "Confirmed",
            "Required",
            "Reset",
            "Expires",
            "Expiry",
            "CreatedAt",
            "UpdatedAt",
            "Present",
            "Provided",
            "Length",
            "Count",
        };

    public static bool IsSensitive(string name)
    {
        if (Names.Contains(name))
        {
            return true;
        }

        // Catch common variants such as UserEmail, CustomerPassword, or
        // RecipientIp without matching unrelated words such as Description.
        foreach (var sensitiveName in Names)
        {
            if (name.EndsWith(
                    sensitiveName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (name.StartsWith(
                    sensitiveName,
                    StringComparison.OrdinalIgnoreCase)
                && !HasSafeSuffix(name))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasSafeSuffix(string name)
    {
        foreach (var safeSuffix in SafeSuffixes)
        {
            if (name.EndsWith(safeSuffix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
