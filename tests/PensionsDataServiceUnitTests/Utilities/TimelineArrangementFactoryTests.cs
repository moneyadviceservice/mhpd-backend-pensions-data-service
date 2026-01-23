using MhpdCommon.Models.MHPDModels;
using PensionsDataService.Utilities;
using System.Text.Json;

namespace PensionsDataServiceUnitTests.Utilities;

public class TimelineArrangementFactoryTests
{
    private readonly IPensionNavigator _navigator = new PensionNavigator();
    private readonly TimelineArrangementFactory _factory;

    public TimelineArrangementFactoryTests()
    {
        _factory = new TimelineArrangementFactory(_navigator);
    }

    [Fact]
    public void Create_Returns_Null_When_HasIncome_IsFalse()
    {
        var pension = new RetrievedPensionRecord
        {
            HasIncome = Boolean.FalseString
        };

        var result = _factory.Create(pension);

        Assert.Null(result);
    }

    [Fact]
    public void Create_Hydrates_Recurring_And_LumpSum_From_Different_Illustrations()
    {
        var pension = new RetrievedPensionRecord
        {
            Id = "P1",
            SchemeName = "Test Scheme",
            PensionType = "DB",
            HasIncome = Boolean.TrueString,
            RetrievalResult = JsonDocument.Parse("""
            {
              "benefitIllustrations": [
                {
                  "illustrationDate": "2024-01-01",
                  "illustrationComponents": [
                    {
                      "illustrationType": "ERI",
                      "payableDetails": {
                        "payableDate": "2035-01-01",
                        "monthlyAmount": 1000,
                        "annualAmount": 12000
                      }
                    }
                  ]
                },
                {
                  "illustrationDate": "2020-01-01",
                  "illustrationComponents": [
                    {
                      "illustrationType": "ERI",
                      "payableDetails": {
                        "payableDate": "2030-01-01",
                        "amount": 40000
                      }
                    }
                  ]
                }
              ]
            }
            """).RootElement
        };

        var arrangement = _factory.Create(pension);

        Assert.NotNull(arrangement);
        Assert.Equal(1000m, arrangement!.MonthlyAmount);
        Assert.Equal(12000m, arrangement.AnnualAmount);
        Assert.Equal(40000m, arrangement.LumpSumAmount);
        Assert.Equal(2035, arrangement.StartYear);
    }
}

