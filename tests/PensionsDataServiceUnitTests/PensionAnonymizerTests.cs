using MhpdCommon.Constants;
using PensionsDataService.Models;
using PensionsDataService.Utilities;

namespace PensionsDataServiceUnitTests;

public class PensionAnonymizerTests
{
    private readonly PensionAnonymizer _sut = new();

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData(" ", " ")]
    public void AnonymizeDates_ReturnsOriginal_WhenInputIsNullOrEmpty(string? input, string? expected)
    {
        var result = _sut.Anonymize(input!);
        Assert.Equal(expected, result);
    }

    [Theory]
    [MemberData(nameof(Snippets.DateTransforms), MemberType = typeof(Snippets))]
    public void AnonymizeDates_AnonymizesDateFields_Correctly(string json, string expectedFragment)
    {
        var result = _sut.Anonymize(json);
        Assert.Contains(expectedFragment, result);
    }

    [Fact]
    public void AnonymizeDates_HandlesNestedObjects()
    {
        var json = $$"""
            {
                "personalDetails": {
                    "contactMethods": [
                        {
                            "contactMethodDetails": {
                                "{{PensionConstants.MembershipStartDate}}": "2010-10-10"
                            }
                        }
                    ]
                }
            }
            """;

        var result = _sut.Anonymize(json);
        Assert.Contains($"\"{Constants.Analytics.MembershipStartDate}\":\"2010-10\"", result);
    }

    [Fact]
    public void Anonymize_HandlesNestedArrays()
    {
        var json = $$"""
            {
                "{{PensionConstants.EmploymentMembershipPeriods}}": [
                    {
                        "{{PensionConstants.MembershipStartDate}}": "2002-02-01",
                        "{{PensionConstants.MembershipEndDate}}": "2005-03-01"
                    },
                    {
                        "{{PensionConstants.MembershipStartDate}}": "2006-01-01",
                        "{{PensionConstants.MembershipEndDate}}": "2007-02-01"
                    }
                ]
            }
            """;

        var result = _sut.Anonymize(json);

        Assert.Contains($"\"{Constants.Analytics.MembershipStartDate}\":\"2002-02\"", result);
        Assert.Contains($"\"{Constants.Analytics.MembershipEndDate}\":\"2005-03\"", result);
        Assert.Contains($"\"{Constants.Analytics.MembershipStartDate}\":\"2006-01\"", result);
        Assert.Contains($"\"{Constants.Analytics.MembershipEndDate}\":\"2007-02\"", result);
    }

    [Fact]
    public void Anonymize_MultipleDatePropertiesInSameObject()
    {
        var json = $$"""
        {
            "{{PensionConstants.DateOfBirth}}": "1980-09-12",
            "{{PensionConstants.StartDate}}": "2000-02-15",
            "{{PensionConstants.RetirementDate}}": "2045-05-30"
        }
        """;

        var result = _sut.Anonymize(json);

        Assert.Contains($"\"{Constants.Analytics.DateOfBirth}\":\"1980\"", result);
        Assert.Contains($"\"{Constants.Analytics.StartDate}\":\"2000-02\"", result);
        Assert.Contains($"\"{Constants.Analytics.RetirementDate}\":\"2045-05\"", result);
    }

    [Fact]
    public void Anonymize_DoesNotAffectSiblingProperties()
    {
        var json = $$"""
        {
            "{{PensionConstants.SchemeName}}": "Test Scheme",
            "{{PensionConstants.RetirementDate}}": "2030-04-01"
        }
        """;

        var result = _sut.Anonymize(json);

        Assert.Contains($"\"{PensionConstants.SchemeName}\":\"Test Scheme\"", result);
        Assert.Contains($"\"{Constants.Analytics.RetirementDate}\":\"2030-04\"", result);
    }

    [Fact]
    public void Anonymize_PreservesStructure_WhileChangingValues()
    {
        var json = $$"""
        {
            "outer": {
                "inner": {
                    "{{PensionConstants.StartDate}}": "2001-04-12",
                    "{{PensionConstants.SchemeName}}": "Test Scheme"
                }
            }
        }
        """;

        var result = _sut.Anonymize(json);

        Assert.Contains("\"outer\":", result);
        Assert.Contains("\"inner\":", result);
        Assert.Contains($"\"{PensionConstants.SchemeName}\":\"Test Scheme\"", result);
        Assert.Contains($"\"{Constants.Analytics.StartDate}\":\"2001-04\"", result);
    }

    [Theory]
    [InlineData("\"name\": \"Trustwell\"", "Trustwell")]
    [InlineData("\"contactMethods\": []", "")]
    public void Anonymize_FlattensTargetToNewProperty(string child, string transformValue)
    {
        var json = $$"""
        {
            "{{PensionConstants.PensionAdministrator}}": {{{child}}}
        }
        """;

        var result = _sut.Anonymize(json);

        Assert.DoesNotContain($"\"{PensionConstants.PensionAdministrator}\":", result);
        Assert.Contains($"\"{Constants.Analytics.PensionAdministrator}\":\"{transformValue}\"", result);
    }

    [Fact]
    public void Anonymize_DoesNothing_WhenFlattenTargetIsNotObject()
    {
        var json = $$"""{"{{PensionConstants.PensionAdministrator}}": "invalid"}""";

        var result = _sut.Anonymize(json);

        Assert.Contains(PensionConstants.PensionAdministrator, result);
        Assert.DoesNotContain(Constants.Analytics.PensionAdministrator, result);
    }
}
