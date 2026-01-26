using MhpdCommon.ViewData;

namespace PensionsDataService.Models
{
    public static class Constants
    {
        public const string HttpGet = "GET";
        public const string HttpDelete = "DELETE";
        public const string LogSource = "Pension Data Service";
        public const string InvalidCorrelationId = "Invalid correlation Id";
        public const string Unknown = "Unknown";

        public static class Analytics
        {
            public const string StartDate = "startDateYYYYMM";
            public const string RetirementDate = "retirementDateYYYYMM";
            public const string DateOfBirth = "yearOfBirthYYYY";
            public const string MembershipStartDate = "membershipStartDateYYYYMM";
            public const string MembershipEndDate = "membershipEndDateYYYYMM";
            public const string PayableDate = "payableDateYYYYMM";
            public const string LastPaymentDate = "lastPaymentDateYYYYMM";
            public const string PensionAdministrator = "pensionAdministratorName";
        }

        public static class PensionTypes
        {
            public static readonly string SP = PensionEnums.PensionType.SP.ToString();
            public const string LU = "LU";
        }

        public static class UnavailableCodes
        {
            public const string DB = "DB";
        }
    }
}
