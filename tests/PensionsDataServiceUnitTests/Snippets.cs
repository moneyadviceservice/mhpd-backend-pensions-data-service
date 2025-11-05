using MhpdCommon.Constants;
using PensionsDataService.Models;

namespace PensionsDataServiceUnitTests;

public static class Snippets
{
    public static IEnumerable<object[]> DateTransforms =>
    [
        [
            $@"{{""{PensionConstants.DateOfBirth}"": ""1980-05-23""}}",
            $"\"{Constants.Analytics.DateOfBirth}\":\"1980\""
        ],
        [
            $@"{{""{PensionConstants.RetirementDate}"": ""2029-03-15""}}",
            $"\"{Constants.Analytics.RetirementDate}\":\"2029-03\""
        ],
        [
            $@"{{""{PensionConstants.PayableDate}"": ""2038-10-10""}}",
            $"\"{Constants.Analytics.PayableDate}\":\"2038-10\""
        ],
        [
            $@"{{""{PensionConstants.LastPaymentDate}"": ""2040-10-10""}}",
            $"\"{Constants.Analytics.LastPaymentDate}\":\"2040-10\""
        ],
        [
            $@"{{""{PensionConstants.StartDate}"": ""2035-06-15""}}",
            $"\"{Constants.Analytics.StartDate}\":\"2035-06\""
        ],
        [
            $@"{{""{PensionConstants.MembershipStartDate}"": ""1980-05-23""}}",
            $"\"{Constants.Analytics.MembershipStartDate}\":\"1980-05\""
        ],
        [
            $@"{{""{PensionConstants.MembershipEndDate}"": ""2032-03-15""}}",
            $"\"{Constants.Analytics.MembershipEndDate}\":\"2032-03\""
        ],
        [
            $@"{{""{PensionConstants.MembershipStartDate}"": ""NotADate""}}",
            $"\"{Constants.Analytics.MembershipStartDate}\":\"NotADate\""
        ]
    ];
}
