namespace CDATokenServices.Models
{
    public static class GrantTypeEnum
    {
        static readonly string UMA = "urn:ietf:params:oauth:grant-type:uma-ticket";        

        public static bool Validate (string grantType, out string badRequestError)
        {
            badRequestError = string.Empty;

            if (string.IsNullOrEmpty(grantType))
            {
                badRequestError = BadRequestModel.InvalidRequest;
                return false;
            }

            if (!(grantType == UMA))
            {
                badRequestError = BadRequestModel.InvalidGrant;
                return false;
            }

            return true;
        }
    }
}