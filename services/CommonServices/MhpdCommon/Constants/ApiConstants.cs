namespace MhpdCommon.Constants;

public class ApiConstants
{
    public const string GuidPattern = @"^[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}$";
    public const string CodeVerifierPattern = @"^[a-zA-Z0-9\-._~]{43,128}$";
    public const string UrlPattern = @"^https?:\/\/([a-zA-Z0-9\-]+\.)+[a-zA-Z]{2,}(:\d{1,5})?(\/[^\s]*)?$";
    public const string JwePattern = @"^[A-Za-z0-9\-_]+(\.[A-Za-z0-9\-_]+){4}$";
    public const string JwtPattern = @"^[A-Za-z0-9\-_]+(\.[A-Za-z0-9\-_]+){2}$";
    public const string RedirectPurpose = "FIND";
}
