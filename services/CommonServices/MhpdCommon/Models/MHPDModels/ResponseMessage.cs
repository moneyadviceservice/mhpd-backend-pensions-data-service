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
        if (string.IsNullOrWhiteSpace(WwwAuthenticateResponseHeader) || 
            !WwwAuthenticateResponseHeader.Contains(tokenToExtract))
        {
            return string.Empty;
        }

        var parts = WwwAuthenticateResponseHeader.Split(tokenToExtract, StringSplitOptions.None);
        if (parts.Length < 2) return string.Empty;

        var valueParts = parts[1].Split(",");
        return valueParts.Length > 0 ? valueParts[0].Replace("\"", "") : string.Empty;
    }
}
