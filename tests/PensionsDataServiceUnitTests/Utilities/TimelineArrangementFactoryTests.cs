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
    public void Create_Returns_Empty_When_HasIncome_IsFalse()
    {
        var pension = new RetrievedPensionRecord
        {
            HasIncome = Boolean.FalseString
        };

        var result = _factory.CreateAll(pension, false);

        Assert.Empty(result);
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
                  "payableDetailsType": "RECURRING",
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
                  "payableDetailsType": "LUMPSUM",
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

        var arrangements = _factory.CreateAll(pension, true).ToList();

        Assert.Equal(2, arrangements.Count);
        Assert.Equal(1000m, arrangements[0].MonthlyAmount);
        Assert.Equal(12000m, arrangements[0].AnnualAmount);
        Assert.Null(arrangements[0].LumpSumAmount);
        Assert.Null(arrangements[0].LumpSumYear);
        Assert.Equal(2035, arrangements[0].StartYear);

        Assert.Null(arrangements[1].MonthlyAmount);
        Assert.Null(arrangements[1].AnnualAmount);
        Assert.Equal(40000m, arrangements[1].LumpSumAmount);
        Assert.Equal(2030, arrangements[1].LumpSumYear);
    }
}

