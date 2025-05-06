using MhpdCommon.Constants;
using MhpdCommon.TokenValidation;
using MhpdCommon.Utils;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ValidationResult = MhpdCommon.TokenValidation.ValidationResult;

namespace MhpdCommon.Models.MessageBodyModels;

public class ClaimsGatheringResponseModel
{
    [RegularExpression(ApiConstants.JwePattern)]
    [JsonPropertyName("ticket")]
    public string Ticket { get; set; } = string.Empty;

    [MinLength(1)]
    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; set; }

    [MinLength(1)]
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [RegularExpression(ApiConstants.UrlPattern)]
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
