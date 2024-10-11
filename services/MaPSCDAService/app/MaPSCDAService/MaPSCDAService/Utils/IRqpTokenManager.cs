using MaPSCDAService.Models;

namespace MaPSCDAService.Utils;

public interface IRqpTokenManager
{
    string GenerateToken(string userSessionId, string iss);
    bool ValidateToken(string token, string iss, out RQPModel rqpModel);
}