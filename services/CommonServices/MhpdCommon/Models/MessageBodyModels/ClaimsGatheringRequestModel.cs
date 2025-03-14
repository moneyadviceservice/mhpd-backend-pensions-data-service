using MhpdCommon.Constants;
using MhpdCommon.Constants.HttpClient;
using MhpdCommon.TokenValidation;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;

namespace MhpdCommon.Models.MessageBodyModels;

public class ClaimsGatheringRequestModel
{
    [FromQuery(Name = QueryParams.Cda.Token.ClientId)]
    public string? ClientId { get; set; }

    [FromQuery(Name = QueryParams.Cda.Token.Ticket)]
    public string? Ticket { get; set; }

    [FromQuery(Name = QueryParams.Cda.ClaimsGathering.ClaimsRedirectUri)]
    public string? ClaimsRedirectUri { get; set; }

    [FromQuery(Name = QueryParams.Cda.ClaimsGathering.RqpToken)]
    public string? Rqp { get; set; }

    [FromQuery(Name = QueryParams.Cda.ClaimsGathering.RequestId)]
    public string? RequestId { get; set; }

    [FromQuery(Name = QueryParams.Cda.ClaimsGathering.State)]
    public string? State { get; set; }

    public ValidationResult IsValidClaimsGatheringRequest()
    {
        if (string.IsNullOrWhiteSpace(ClientId)) return ValidationResult.Failure(TokenValidationMessages.InvalidClientIdFormat);
        if (string.IsNullOrWhiteSpace(RequestId)) return ValidationResult.Failure(TokenValidationMessages.InvalidRequestId);
        if (string.IsNullOrWhiteSpace(State)) return ValidationResult.Failure(TokenValidationMessages.InvalidState);

        if (ClaimsRedirectUri == null || !TokenUtility.IsValidUrl(ClaimsRedirectUri)) 
            return ValidationResult.Failure(TokenValidationMessages.InvalidClaimiRedirectUri);
        if (Rqp == null || !JwtValidator.IsJwtFormatValid(Rqp))
            return ValidationResult.Failure(TokenValidationMessages.RqpNotAJwt);
        if (Ticket == null || !JweValidator.IsJweFormatValid(Ticket))
            return ValidationResult.Failure(TokenValidationMessages.InvalidJweTicketQueryFormat);
        if (!string.Equals(Ticket, SecurityConstants.Jwe.ClaimsRequiredPermissionTicket))
            return ValidationResult.Failure(TokenValidationMessages.InvalidPermissionTicket);

        return ValidationResult.Success();
    }
}
