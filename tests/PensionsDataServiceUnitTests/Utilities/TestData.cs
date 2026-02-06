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
                      "payableDetailsType": "LUMPSUM",
                      "illustrationComponents": [
                        {
                          "illustrationType": "ERI",
                          "payableDetails": {
                            "payableDate": "2030-01-01",
                            "amount": 50000
                          }
                        },
                        {
                          "illustrationType": "AP",
                          "payableDetails": {
                            "payableDate": "2030-02-01",
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
                      "payableDetailsType": "RECURRING",
                      "illustrationComponents": [
                        {
                          "illustrationType": "ERI",
                          "payableDetails": {
                            "payableDate": "2035-01-01",
                            "monthlyAmount": 1500,
                            "annualAmount": 18000
                          }
                        },
                        {
                          "illustrationType": "AP",
                          "payableDetails": {
                            "payableDate": "2035-02-01",
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

    public static RetrievedPensionRecord CreateConfirmedPensionWithDetailData()
    {
        var json = """
        {
          "retirementDate": "2040-01-01",
          "benefitIllustrations": [
            {
              "illustrationDate": "2025-01-01",
              "payableDetailsType": "RECURRING",
              "illustrationComponents": [
                {
                  "illustrationType": "ERI",
                  "benefitType": "DB",
                  "payableDetails": {
                    "monthlyAmount": 3200,
                    "annualAmount": 38400,
                    "payableDate": "2040-02-01"
                  },
                  "illustrationWarnings": ["PSO", "PNR"]
                },
                {
                  "illustrationType": "AP",
                  "dcPot": 125000,
                  "payableDetails": {
                    "monthlyAmount": 3200,
                    "annualAmount": 38400,
                    "payableDate": "2040-03-01"
                  }
                }
              ]
            },
            {
              "illustrationDate": "2022-01-01",
              "payableDetailsType": "LUMPSUM",
              "illustrationComponents": [
                {
                  "illustrationType": "ERI",
                  "payableDetails": {
                    "amount": 25000,
                    "payableDate": "2040-04-01"
                  }
                },
                {
                  "illustrationType": "AP",
                  "dcPot": 125000,
                  "payableDetails": {
                    "amount": 25000,
                    "payableDate": "2040-05-01"
                  }
                }
              ]
            }
          ]
        }
        """;

        return new RetrievedPensionRecord
        {
            AssetId = "pension-1",
            Category = Category.Confirmed,
            RetrievalResult = JsonSerializer.Deserialize<JsonElement>(json)
        };
    }

    public static RetrievedPensionRecord CreatePensionWithWarnings()
        => CreateConfirmedPensionWithDetailData();

    public static RetrievedPensionRecord CreatePensionWithoutLumpSum()
    {
        var json = """
        {
          "benefitIllustrations": [
            {
              "illustrationDate": "2025-01-01",
              "payableDetailsType": "RECURRING",
              "illustrationComponents": [
                {
                  "illustrationType": "ERI",
                  "benefitType": "DB",
                  "payableDetails": {
                    "monthlyAmount": 3000,
                    "payableDate": "2038-01-01"
                  }
                },
                {
                  "illustrationType": "AP",
                  "benefitType": "DB",
                  "payableDetails": {
                    "monthlyAmount": 3000,
                    "payableDate": "2038-02-01"
                  }
                }
              ]
            }
          ]
        }
        """;

        return new RetrievedPensionRecord
        {
            AssetId = "pension-2",
            Category = Category.Confirmed,
            RetrievalResult = JsonSerializer.Deserialize<JsonElement>(json)
        };
    }

    public static RetrievedPensionRecord CreateDCPension()
    {
        var json = """
        {
          "benefitIllustrations": [
            {
              "illustrationDate": "2025-01-01",
              "payableDetailsType": "RECURRING",
              "illustrationComponents": [
                {
                  "illustrationType": "ERI",
                  "benefitType": "DC",
                  "dcPot": 125000,
                  "payableDetails": {
                    "monthlyAmount": 3000,
                    "payableDate": "2038-01-01"
                  }
                },
                {
                  "illustrationType": "AP",
                  "benefitType": "DC",
                  "payableDetails": {
                    "monthlyAmount": 3000,
                    "payableDate": "2038-02-01"
                  }
                }
              ]
            }
          ]
        }
        """;

        return new RetrievedPensionRecord
        {
            AssetId = "pension-2",
            Category = Category.Confirmed,
            RetrievalResult = JsonSerializer.Deserialize<JsonElement>(json)
        };
    }

    public static RetrievedPensionRecord CreateAVCPension()
    {
        var json = """
        {
          "pensionType": "AVC",
          "benefitIllustrations": [
            {
              "illustrationDate": "2025-01-01",
              "payableDetailsType": "NOPAYMENT",
              "illustrationComponents": [
                {
                  "illustrationType": "ERI",
                  "payableDetails": {
                    "reason": "SML",
                    "payableDate": "2038-01-01"
                  }
                },
                {
                  "illustrationType": "AP",
                  "payableDetails": {
                    "reason": "SML",
                    "payableDate": "2038-02-01"
                  }
                }
              ]
            }
          ]
        }
        """;

        return new RetrievedPensionRecord
        {
            AssetId = "pension-2",
            Category = Category.Confirmed,
            RetrievalResult = JsonSerializer.Deserialize<JsonElement>(json)
        };
    }

    public static RetrievedPensionRecord CreateHYBPension(string benefitType)
    {
        var json = $$"""
        {"pensionType": "HYB",
          "benefitIllustrations": [
            {
              "illustrationDate": "2025-01-01",
              "payableDetailsType": "RECURRING",
              "illustrationComponents": [
                {
                  "illustrationType": "ERI",
                  "dcPot": 125000,
                  "benefitType": "{{benefitType}}",
                  "payableDetails": {
                    "monthlyAmount": 3000,
                    "payableDate": "2038-01-01"
                  }
                },
                {
                  "illustrationType": "AP",
                  "dcPot": 125000,
                  "benefitType": "{{benefitType}}",
                  "payableDetails": {
                    "monthlyAmount": 3000,
                    "payableDate": "2038-02-01"
                  }
                }
              ]
            }
          ]
        }
        """;

        return new RetrievedPensionRecord
        {
            AssetId = "pension-2",
            Category = Category.Confirmed,
            RetrievalResult = JsonSerializer.Deserialize<JsonElement>(json)
        };
    }

    public static RetrievedPensionRecord CreatePensionWithoutIllustrations()
    {
        return new RetrievedPensionRecord
        {
            AssetId = "pension-3",
            Category = Category.Confirmed,
            RetrievalResult = JsonSerializer.Deserialize<JsonElement>("{}")
        };
    }
}
