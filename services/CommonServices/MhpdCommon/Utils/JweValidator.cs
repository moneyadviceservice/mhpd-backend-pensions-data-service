using System.Text.RegularExpressions;

namespace MhpdCommon.Utils
{
    public static class JweValidator
    {
        public static bool IsJweFormatValid(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            var parts = token.Split('.');
            if (parts.Length != 5)
            {
                return false; // JWE must have 5 parts
            }

            return parts.All(IsBase64Url);
        }

        private static bool IsBase64Url(string base64Url)
        {
            if (string.IsNullOrEmpty(base64Url))
            {
                return false;
            }

            // Check for valid Base64 URL characters
            if (!Regex.IsMatch(base64Url, @"^[A-Za-z0-9_-]+$"))
            {
                return false;
            }

            // Validate length and padding
            var padding = base64Url.Length % 4;
            var padded = base64Url + new string('=', (4 - padding) % 4); // Add necessary padding
            try
            {
                Convert.FromBase64String(padded.Replace('-', '+').Replace('_', '/'));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}