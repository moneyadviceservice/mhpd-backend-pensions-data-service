namespace CDATokenServices.Models
{
    public class ClaimTokenFormatEnum
    {
        public const string PensionDashboadRqp = "pension_dashboad_rqp";

        public static bool Validate(string claimTokenFormat, out string badRequestError)
        {
            badRequestError = string.Empty;
            if (string.IsNullOrEmpty(claimTokenFormat))
            {
                badRequestError = BadRequestModel.InvalidRequest;
                return false;
            }

            if (!(claimTokenFormat == PensionDashboadRqp))
            {
                badRequestError = BadRequestModel.InvalidRequest;
                return false;
            }
            return true;

        }
    }
}
