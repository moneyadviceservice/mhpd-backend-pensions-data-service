using MhpdCommon.Constants;
using PensionsDataService.Models;
using System.Globalization;
using System.Text.Json.Nodes;

namespace PensionsDataService.Utilities;

public class PensionAnonymizer : IPensionAnonymizer
{
    private static readonly Dictionary<string, string> DateProperties = new(StringComparer.Ordinal)
    {
        { PensionConstants.DateOfBirth, Constants.Analytics.DateOfBirth },
        { PensionConstants.StartDate, Constants.Analytics.StartDate },
        { PensionConstants.RetirementDate, Constants.Analytics.RetirementDate },
        { PensionConstants.MembershipStartDate, Constants.Analytics.MembershipStartDate },
        { PensionConstants.MembershipEndDate, Constants.Analytics.MembershipEndDate },
        { PensionConstants.PayableDate, Constants.Analytics.PayableDate },
        { PensionConstants.LastPaymentDate, Constants.Analytics.LastPaymentDate }
    };

    private static readonly List<TransformRule> TransformRules =
    [
        new TransformRule
        {
            SourceProperty = PensionConstants.PensionAdministrator,
            TargetProperty = Constants.Analytics.PensionAdministrator,
            ExtractProperty = "name"
        }
    ];

    public string Anonymize(string pensionData)
    {
        if (string.IsNullOrWhiteSpace(pensionData))
        {
            return pensionData;
        }

        var node = JsonNode.Parse(pensionData);
        Anonymize(node);

        return node?.ToJsonString() ?? pensionData;
    }

    private static void Anonymize(JsonNode? node)
    {
        if (node is null)
        {
            return;
        }

        if (node is JsonObject obj)
        {
            Anonymize(obj);
        }
        else if (node is JsonArray arr)
        {
            foreach (var item in arr)
            {
                Anonymize(item);
            }
        }
    }

    private static void Anonymize(JsonObject obj)
    {
        foreach (var (key, value) in obj.ToList())
        {
            if (value is JsonObject child &&
                TryFlattenObject(key, child, obj))
            {
                continue;
            }

            switch (value)
            {
                case JsonValue val:

                    if (IsDateProperty(key))
                    {
                        var raw = ExtractValue(val);
                        var format = string.Equals(key, PensionConstants.DateOfBirth) ? "yyyy" : "yyyy-MM";
                        var anonymised = AnonymizeDateString(raw, format);
                        obj.Remove(key);
                        obj.Add(DateProperties[key], anonymised);
                    }
                    break;

                case JsonObject or JsonArray:
                    Anonymize(value);
                    break;
            }
        }
    }

    private static bool IsDateProperty(string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return false;
        }

        return DateProperties.ContainsKey(propertyName);
    }

    private static string AnonymizeDateString(string original, string format)
    {
        if (DateTime.TryParse(original, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
        {
            return new DateTime(dt.Year, dt.Month, 1, 0, 0, 0, DateTimeKind.Unspecified).ToString(format);
        }

        return original;
    }

    private static string ExtractValue(JsonValue value)
    {
        if (!value.TryGetValue<string>(out var raw))
        {
            raw = value.ToJsonString().Trim('"');
        }

        return raw;
    }

    private static bool TryFlattenObject(string key, JsonObject obj, JsonObject parent)
    {
        var rule = TransformRules.FirstOrDefault(r => r.SourceProperty == key);
        if (rule is null)
        {
            return false;
        }

        string? extracted = null;

        if (obj[rule.ExtractProperty] is JsonValue value &&
            value.TryGetValue<string>(out var raw) &&
            !string.IsNullOrWhiteSpace(raw))
        {
            extracted = raw;
        }

        parent.Remove(key);
        parent.Add(rule.TargetProperty, extracted ?? string.Empty);

        return true;
    }
}
