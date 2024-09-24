namespace MhpdCommon.Utils;

public static class JwtValidator
{
    public static bool IsJwtFormatValid(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return false;
        }

        var parts = token.Split('.');
        if (parts.Length != 3)
        {
            return false; // A valid JWT should have 3 parts
        }

        // Validate the header and payload using IsBase64Url
        return IsBase64Url(parts[0]) && IsBase64Url(parts[1]) && IsBase64Url(parts[2]);
    }

    private static bool IsBase64Url(string base64Url)
    {
        if (string.IsNullOrEmpty(base64Url))
        {
            return false;
        }

        // Check for valid Base64 URL characters
        if (base64Url.Any(c => !(c is >= 'A' and <= 'Z' || c is >= 'a' and <= 'z' || c is >= '0' and <= '9' || c == '-' || c == '_' || c == '.')))
        {
            return false;
        }

        // Validate length and padding
        var padding = base64Url.Length % 4;
        switch (padding)
        {
            case 2:
                base64Url += "==";
                break;
            case 3:
                base64Url += "=";
                break;
        }

        return base64Url.Length % 4 == 0;
    }
}