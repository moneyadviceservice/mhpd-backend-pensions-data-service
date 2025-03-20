using MhpdCommon.Constants;
using System.Net;

namespace MhpdCommon.Models.MHPDModels;

public class ResponseMessage
{
    public HttpStatusCode ResponseStatusCode { get; set; }

    public string? WwwAuthenticateResponseHeader { get; set; }

    public string Ticket => ExtractWwwAuthenticateHeaderValue(HeaderConstants.AuthenticateTicket);

    public string AsUri => ExtractWwwAuthenticateHeaderValue(HeaderConstants.AuthenticateUri);

    private string ExtractWwwAuthenticateHeaderValue(string tokenToExtract)
    {
        if (string.IsNullOrWhiteSpace(WwwAuthenticateResponseHeader))
        {
            return string.Empty;
        }

        var token = WwwAuthenticateResponseHeader.Split(tokenToExtract)[1];
        return token.Split(",")[0].Replace("\"", "");
    }
}
