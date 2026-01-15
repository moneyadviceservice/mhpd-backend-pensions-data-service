using PensionsDataService.Utilities;
using System.Text.Json.Nodes;

namespace PensionsDataServiceUnitTests.Utilities;

public class PensionNavigatorTests
{
    private readonly PensionNavigator _navigator = new();

    [Fact]
    public void SelectLatestIllustration_PicksLatestByDate()
    {
        var node = JsonNode.Parse("""
        {
          "benefitIllustrations": [
            { "illustrationDate": "2020-01-01" },
            { "illustrationDate": "2024-01-01" },
            { "illustrationDate": "2022-01-01" }
          ]
        }
        """)!;

        var illustration = _navigator.SelectLatestIllustration(node);
        var result = illustration!["illustrationDate"]!.GetValue<string>();

        Assert.Equal("2024-01-01", result);
    }

    [Fact]
    public void SelectLatestIllustration_Tie_PicksFirst()
    {
        var node = JsonNode.Parse("""
        {
          "benefitIllustrations": [
            { "illustrationDate": "2024-01-01", "id": 1 },
            { "illustrationDate": "2024-01-01", "id": 2 }
          ]
        }
        """)!;

        var illustration = _navigator.SelectLatestIllustration(node);
        var result = illustration!["id"]!.GetValue<int>();

        Assert.Equal(1, result);
    }

    [Fact]
    public void SelectLatestIllustration_NoValidDates_PicksFirst()
    {
        var node = JsonNode.Parse("""
        {
          "benefitIllustrations": [
            { "foo": "bar" },
            { "baz": "qux" }
          ]
        }
        """)!;

        var illustration = _navigator.SelectLatestIllustration(node);
        var result = illustration!["foo"]!.GetValue<string>();

        Assert.Equal("bar", result);
    }

    [Fact]
    public void SelectLatestIllustration_EmptyArray_ReturnsNull()
    {
        var node = JsonNode.Parse("""
        { "benefitIllustrations": [] }
        """)!;

        var result = _navigator.SelectLatestIllustration(node);

        Assert.Null(result);
    }

    [Fact]
    public void SelectEarliestComponent_PicksEarliestPayableDate()
    {
        var node = JsonNode.Parse("""
        {
          "illustrationComponents": [
            { "illustrationType": "ERI", "payableDetails": { "payableDate": "2035-01-01" }},
            { "illustrationType": "ERI", "payableDetails": { "payableDate": "2030-01-01" }},
            { "illustrationType": "ERI", "payableDetails": { "payableDate": "2040-01-01" }}
          ]
        }
        """)!;

        var component = _navigator.SelectEarliestComponent(node, "ERI");
        var result = component!["payableDetails"]!["payableDate"]!.GetValue<string>();
        Assert.Equal("2030-01-01", result);
    }

    [Fact]
    public void SelectIllustrationComponent_Tie_PicksFirst()
    {
        var node = JsonNode.Parse("""
        {
          "benefitIllustrations": [
            { "illustrationDate": "2024-01-01", "illustrationComponents": [
                { "illustrationType": "ERI", "id": 1, "payableDetails": { "payableDate": "2030-01-01" }},
                { "illustrationType": "ERI", "id": 2, "payableDetails": { "payableDate": "2030-01-01" }}
            ] }
          ]
        }
        """)!;

        var component = _navigator.SelectIllustrationComponent(node);
        var result = component!["id"]!.GetValue<int>();
        Assert.Equal(1, result);
    }

    [Fact]
    public void SelectIllustrationComponent_EriIsDB_PicksAP()
    {
        var node = JsonNode.Parse("""
        {
          "benefitIllustrations": [
            { "illustrationDate": "2024-01-01", "illustrationComponents": [
                { "illustrationType": "ERI", "unavailableReason": "DB", "payableDetails": { "payableDate": "2030-01-01" }},
                { "illustrationType": "AP", "payableDetails": { "payableDate": "2030-01-01" }}
            ] }
          ]
        }
        """)!;

        var component = _navigator.SelectIllustrationComponent(node);
        var result = component!["illustrationType"]!.GetValue<string>();
        Assert.Equal("AP", result);
    }

    [Fact]
    public void SelectEarliestComponent_NoDates_PicksFirst()
    {
        var node = JsonNode.Parse("""
        {
          "illustrationComponents": [
            { "illustrationType": "ERI", "id": 1 },
            { "illustrationType": "ERI", "id": 2 }
          ]
        }
        """)!;

        var component = _navigator.SelectEarliestComponent(node, "ERI");
        var result = component!["id"]!.GetValue<int>();
        Assert.Equal(1, result);
    }

    [Fact]
    public void SelectEarliestComponent_NoMatchingType_ReturnsNull()
    {
        var node = JsonNode.Parse("""
        {
          "illustrationComponents": [
            { "illustrationType": "AP" }
          ]
        }
        """)!;

        var result = _navigator.SelectEarliestComponent(node, "ERI");

        Assert.Null(result);
    }

    #region SelectRetirementDate

    [Fact]
    public void SelectRetirementDate_UsesPayableDate_WhenPresent()
    {
        var retrieval = Parse("""{ "retirementDate": "2040-01-01" }""");
        var component = Parse("""
        {
            "payableDetails": {
                "payableDate": "2035-06-01"
            }
        }
        """);

        var result = _navigator.SelectRetirementDate(retrieval, component);

        Assert.Equal(new DateTime(2035, 6, 1, 0, 0, 0, DateTimeKind.Unspecified), result);
    }

    [Fact]
    public void SelectRetirementDate_FallsBackToTopLevel_WhenComponentMissing()
    {
        var retrieval = Parse("""{ "retirementDate": "2040-01-01" }""");

        var result = _navigator.SelectRetirementDate(retrieval, null);

        Assert.Equal(new DateTime(2040, 1, 1, 0, 0, 0, DateTimeKind.Unspecified), result);
    }

    [Fact]
    public void SelectRetirementDate_ReturnsNull_WhenNoDates()
    {
        var retrieval = Parse("""{ }""");

        var result = _navigator.SelectRetirementDate(retrieval, null);

        Assert.Null(result);
    }

    #endregion

    #region SelectMonthlyAmount

    [Fact]
    public void SelectMonthlyAmount_ReturnsValue_WhenPresent()
    {
        var component = Parse("""
        {
            "payableDetails": {
                "monthlyAmount": 1234.56
            }
        }
        """);

        var result = _navigator.SelectMonthlyAmount(component);

        Assert.Equal(1234.56m, result);
    }

    [Fact]
    public void SelectMonthlyAmount_ReturnsNull_WhenMissing()
    {
        var component = Parse("""{ }""");

        var result = _navigator.SelectMonthlyAmount(component);

        Assert.Null(result);
    }

    [Fact]
    public void SelectMonthlyAmount_ReturnsNull_WhenComponentNull()
    {
        var result = _navigator.SelectMonthlyAmount(null);

        Assert.Null(result);
    }

    #endregion

    #region SelectAnnualAmount

    [Fact]
    public void SelectAnnualAmount_ReturnsValue_WhenPresent()
    {
        var component = Parse("""
        {
            "payableDetails": {
                "annualAmount": 24000
            }
        }
        """);

        var result = _navigator.SelectAnnualAmount(component);

        Assert.Equal(24000m, result);
    }

    [Fact]
    public void SelectAnnualAmount_ReturnsNull_WhenMissing()
    {
        var component = Parse("""{ }""");

        var result = _navigator.SelectAnnualAmount(component);

        Assert.Null(result);
    }

    #endregion

    #region SelectUnavailableCode

    [Fact]
    public void SelectUnavailableCode_ReturnsValue_WhenPresent()
    {
        var component = Parse("""
        {
            "unavailableReason": "PPF"
        }
        """);

        var result = _navigator.SelectUnavailableCode(component);

        Assert.Equal("PPF", result);
    }

    [Fact]
    public void SelectUnavailableCode_ReturnsNull_WhenMissing()
    {
        var component = Parse("""{ }""");

        var result = _navigator.SelectUnavailableCode(component);

        Assert.Null(result);
    }

    #endregion

    #region GetPayableDetails

    [Fact]
    public void GetPayableDetails_PopulatesAllFields_WhenPresent()
    {
        var component = Parse("""
        {
            "payableDetails": {
                "monthlyAmount": 1000,
                "annualAmount": 12000,
                "payableDate": "2035-01-01",
                "lastPaymentDate": "2065-01-01",
                "reason": "N/A"
            }
        }
        """);

        var result = _navigator.GetPayableDetails(component);

        Assert.Equal(1000m, result.MonthlyAmount);
        Assert.Equal(12000m, result.AnnualAmount);
        Assert.Equal(new DateTime(2035, 1, 1, 0, 0, 0, DateTimeKind.Unspecified), result.PayableDate);
        Assert.Equal(new DateTime(2065, 1, 1, 0, 0, 0, DateTimeKind.Unspecified), result.LastPaymentDate);
        Assert.Equal("N/A", result.AmountNotProvidedReason);
    }

    [Fact]
    public void GetPayableDetails_ReturnsAllNulls_WhenComponentNull()
    {
        var result = _navigator.GetPayableDetails(null);

        Assert.Null(result.MonthlyAmount);
        Assert.Null(result.AnnualAmount);
        Assert.Null(result.PayableDate);
        Assert.Null(result.LastPaymentDate);
        Assert.Null(result.AmountNotProvidedReason);
    }

    [Fact]
    public void GetPayableDetails_HandlesPartialData()
    {
        var component = Parse("""
        {
            "payableDetails": {
                "monthlyAmount": 500
            }
        }
        """);

        var result = _navigator.GetPayableDetails(component);

        Assert.Equal(500m, result.MonthlyAmount);
        Assert.Null(result.AnnualAmount);
        Assert.Null(result.PayableDate);
        Assert.Null(result.LastPaymentDate);
        Assert.Null(result.AmountNotProvidedReason);
    }

    #endregion

    private static JsonNode Parse(string json) => JsonNode.Parse(json)!;
}

