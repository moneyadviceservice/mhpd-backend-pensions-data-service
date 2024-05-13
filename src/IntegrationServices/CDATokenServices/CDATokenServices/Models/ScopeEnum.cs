namespace CDATokenServices.Models
{
    public static class ScopeEnum
    {
        public static string Owner = "owner";
      
        public static bool Validate(string scope, out string badRequestError)
        {
            badRequestError = String.Empty;
            if (string.IsNullOrEmpty(scope))
            {
                badRequestError = BadRequestModel.InvalidRequest;
                return false;
            }
            if (!(scope == Owner))
            {
                badRequestError = BadRequestModel.InvalidScope;
                return false;
            }
            return true;
        }
    }
}