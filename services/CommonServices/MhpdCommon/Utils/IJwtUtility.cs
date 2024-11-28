using Microsoft.Extensions.Logging;

namespace MhpdCommon.Utils;

public interface IJwtUtility
{
    public Task ValidateJwtTokenWithKidAsync(string idToken, ILogger logger);
}