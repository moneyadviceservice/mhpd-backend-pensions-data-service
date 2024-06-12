namespace PeiIntegrationService.Models.PeiIntegrationService
{
    public class ScopeEnum
    {
        public const string Owner = "owner";

        public static bool Validate(string scope)
        {
            if (string.IsNullOrEmpty(scope) || scope != Owner)
            {
                return false;
            }

            return true;
        }
    }
}