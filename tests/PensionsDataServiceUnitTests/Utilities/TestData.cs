using MhpdCommon.Models.MHPDModels;
using System.Text.Json;
using static MhpdCommon.ViewData.EvaluationConstants;

namespace PensionsDataServiceUnitTests.Utilities;

internal static class TestData
{
    public static IEnumerable<RetrievedPensionRecord> CreateTwoOverlappingPensions()
    {
        return
        [
            new RetrievedPensionRecord
            {
                Id = "P1",
                SchemeName = "First Scheme",
                PensionType = "DB",
                HasIncome = Boolean.TrueString,
                Category = Category.Confirmed,
                RetrievalResult = JsonDocument.Parse("""
                {
                  "benefitIllustrations": [
                    {
                      "illustrationDate": "2024-01-01",
                      "illustrationComponents": [
                        {
                          "illustrationType": "ERI",
                          "payableDetails": {
                            "payableDate": "2030-01-01",
                            "monthlyAmount": 1000,
                            "annualAmount": 12000
                          }
                        }
                      ]
                    }
                  ]
                }
                """).RootElement
            },

            new RetrievedPensionRecord
            {
                Id = "P2",
                SchemeName = "Second Scheme",
                PensionType = "DB",
                HasIncome = Boolean.TrueString,
                Category = Category.Confirmed,
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
                            "monthlyAmount": 1500,
                            "annualAmount": 18000
                          }
                        }
                      ]
                    }
                  ]
                }
                """).RootElement
            }
        ];
    }

    public static IEnumerable<RetrievedPensionRecord> CreatePensionsContainingLumpSum()
    {
        return
        [
            new RetrievedPensionRecord
            {
                Id = "LS1",
                SchemeName = "Lump Sum Scheme",
                PensionType = "DB",
                HasIncome = Boolean.TrueString,
                Category = Category.Confirmed,
                RetrievalResult = JsonDocument.Parse("""
                {
                  "benefitIllustrations": [
                    {
                      "illustrationDate": "2020-01-01",
                      "illustrationComponents": [
                        {
                          "illustrationType": "ERI",
                          "payableDetails": {
                            "payableDate": "2030-01-01",
                            "amount": 50000
                          }
                        }
                      ]
                    }
                  ]
                }
                """).RootElement
            },

            new RetrievedPensionRecord
            {
                Id = "P1",
                SchemeName = "First Scheme",
                PensionType = "DB",
                HasIncome = Boolean.TrueString,
                Category = Category.Confirmed,
                RetrievalResult = JsonDocument.Parse("""
                {
                  "benefitIllustrations": [
                    {
                      "illustrationDate": "2024-01-01",
                      "illustrationComponents": [
                        {
                          "illustrationType": "ERI",
                          "payableDetails": {
                            "payableDate": "2030-01-01",
                            "lastPaymentDate": "2060-01-01",
                            "monthlyAmount": 1000,
                            "annualAmount": 12000
                          }
                        }
                      ]
                    }
                  ]
                }
                """).RootElement
            }
        ];
    }

    public static IEnumerable<RetrievedPensionRecord> CreateMultiplePensions()
    {
        return
        [
            new RetrievedPensionRecord
            {
                Id = "P1",
                SchemeName = "First Scheme",
                PensionType = "DB",
                HasIncome = Boolean.TrueString,
                Category = Category.Confirmed,
                RetrievalResult = JsonDocument.Parse("""
                {
                  "benefitIllustrations": [
                    {
                      "illustrationDate": "2024-01-01",
                      "illustrationComponents": [
                        {
                          "illustrationType": "ERI",
                          "payableDetails": {
                            "payableDate": "2030-01-01",
                            "lastPaymentDate": "2060-01-01",
                            "monthlyAmount": 1000,
                            "annualAmount": 12000
                          }
                        }
                      ]
                    }
                  ]
                }
                """).RootElement
            },

            new RetrievedPensionRecord
            {
                Id = "P2",
                SchemeName = "Second Scheme",
                PensionType = "DC",
                HasIncome = Boolean.TrueString,
                Category = Category.Confirmed,
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
                            "monthlyAmount": 1500,
                            "annualAmount": 18000
                          }
                        }
                      ]
                    }
                  ]
                }
                """).RootElement
            },

            new RetrievedPensionRecord
            {
                Id = "LS1",
                SchemeName = "Lump Sum Scheme",
                PensionType = "DB",
                HasIncome = Boolean.TrueString,
                Category = Category.Confirmed,
                RetrievalResult = JsonDocument.Parse("""
                {
                  "benefitIllustrations": [
                    {
                      "illustrationDate": "2020-01-01",
                      "illustrationComponents": [
                        {
                          "illustrationType": "ERI",
                          "payableDetails": {
                            "payableDate": "2030-01-01",
                            "amount": 50000
                          }
                        }
                      ]
                    }
                  ]
                }
                """).RootElement
            },

            new RetrievedPensionRecord
            {
                Id = "SP",
                SchemeName = "State Pension",
                PensionType = "SP",
                HasIncome = Boolean.TrueString,
                Category = Category.Confirmed,
                RetrievalResult = JsonDocument.Parse("""
                {
                  "benefitIllustrations": [
                    {
                      "illustrationDate": "2020-01-01",
                      "illustrationComponents": [
                        {
                          "illustrationType": "ERI",
                          "payableDetails": {
                            "payableDate": "2035-01-01",
                            "monthlyAmount": 1500,
                            "annualAmount": 18000
                          }
                        }
                      ]
                    }
                  ]
                }
                """).RootElement
            }
        ];
    }
}
