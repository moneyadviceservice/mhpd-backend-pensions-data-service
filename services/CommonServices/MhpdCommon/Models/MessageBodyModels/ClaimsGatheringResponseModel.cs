using MhpdCommon.TokenValidation;
using MhpdCommon.Utils;
using System.Text.Json.Serialization;

namespace MhpdCommon.Models.MessageBodyModels;

public class ClaimsGatheringResponseModel
{
    [JsonPropertyName("ticket")]
    public string Ticket { get; set; } = string.Empty;

    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("redirect_user")]
    public string RedirectUser { get; set; } = string.Empty;

    public ValidationResult IsValidClaimsGatheringResponse()
    {
        if (RedirectUser == null || !TokenUtility.IsValidUrl(RedirectUser))
        {
            return ValidationResult.Failure(TokenValidationMessages.InvalidClaimiRedirectUri);
        }

        if (Ticket == null || !JweValidator.IsJweFormatValid(Ticket))
        {
            return ValidationResult.Failure(TokenValidationMessages.InvalidJweTicketQueryFormat);
        }

        return ValidationResult.Success();
    }
}
