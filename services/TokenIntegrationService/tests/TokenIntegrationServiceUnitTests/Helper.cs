using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using MhpdCommon.Utils;
using Microsoft.Extensions.Logging;
using Moq;

namespace TokenIntegrationServiceUnitTests;

public static class Helper
{
    
    public static List<ITokenRequestValidator<CdaTokenRequestModel>> GetOrderedValidators()
    {
        // Mock loggers for each validator
        var logger2 = new Mock<ILogger<UnsupportedGrantTypeValidation>>();
        var logger3 = new Mock<ILogger<ClaimTokenFormatNotPresentValidator>>();
        var logger4 = new Mock<ILogger<ClaimTokenFormatNotPensionDashboardRqpValidator>>();
        var logger5 = new Mock<ILogger<ScopeNotOwnerValidator>>();
        var logger6 = new Mock<ILogger<ScopeNotPresentValidator>>();
        var logger7 = new Mock<ILogger<TicketNotAJwtValidator>>();
        var logger8 = new Mock<ILogger<TicketQueryNotPresentValidator>>();
        var logger9 = new Mock<ILogger<ClientIdNotPresentValidation>>();
        var logger10 = new Mock<ILogger<ClientIdInvalidFormatValidation>>();
        var logger11 = new Mock<ILogger<ClientSecretNotPresentValidation>>();
        var logger12 = new Mock<ILogger<ClientSecretNotGuidValidation>>();
        var logger13 = new Mock<ILogger<CodeNotPresentValidation>>();
        var logger14 = new Mock<ILogger<CodeVerifierNotPresentValidation>>();
        var logger15 = new Mock<ILogger<CodeVerifierNotBase64String>>();
        var logger16 = new Mock<ILogger<RedirectUriNotPresentValidation>>();
        var logger17 = new Mock<ILogger<RedirectUriNotValidUrlValidation>>();
        var logger18 = new Mock<ILogger<UnsupportedGrantTypeValidation>>();
        var logger19 = new Mock<ILogger<CodeInvalidFormatValidation>>();
        var logger20 = new Mock<ILogger<ClaimTokenNotPresentValidation>>();
        var logger21 = new Mock<ILogger<ClaimTokenNotJwtValidator>>();
        var logger22 = new Mock<ILogger<GrantTypeNotPresentValidator>>();
            
        // Mock Id validators
        var idValidator = new Mock<IdValidator>();

        // Create the validators
        var validators = new List<ITokenRequestValidator<CdaTokenRequestModel>>
        {
            new UnsupportedGrantTypeValidation(logger2.Object),
            new ClaimTokenFormatNotPresentValidator(logger3.Object),
            new ClaimTokenFormatNotPensionDashboardRqpValidator(logger4.Object),
            new ScopeNotOwnerValidator(logger5.Object),
            new ScopeNotPresentValidator(logger6.Object),
            new TicketNotAJwtValidator(logger7.Object),
            new TicketQueryNotPresentValidator(logger8.Object),
            new ClientIdNotPresentValidation(logger9.Object),
            new ClientIdInvalidFormatValidation(logger10.Object),
            new ClientSecretNotPresentValidation(logger11.Object),
            new ClientSecretNotGuidValidation(logger12.Object, idValidator.Object),
            new CodeNotPresentValidation(logger13.Object),
            new CodeVerifierNotPresentValidation(logger14.Object),
            new CodeVerifierNotBase64String(logger15.Object),
            new RedirectUriNotPresentValidation(logger16.Object),
            new RedirectUriNotValidUrlValidation(logger17.Object),
            new UnsupportedGrantTypeValidation(logger18.Object),
            new CodeInvalidFormatValidation(logger19.Object),
            new ClaimTokenNotPresentValidation(logger20.Object),
            new ClaimTokenNotJwtValidator(logger21.Object),
            new GrantTypeNotPresentValidator(logger22.Object)
        };

        // Return the ordered validators based on the Order property
        return validators.OrderBy(v => v.Order).ToList();
    }

    // Helper method to create and order validators
    public static List<ITokenRequestValidator<TokenIntegrationRequestModel>> GetOrderedValidatorsForTokenIntegrationRequest()
    {
        // Mock loggers for each validator
        var logger2 = new Mock<ILogger<RqpNotPresentValidator>>();
        var logger3 = new Mock<ILogger<RqpNotAJwtValidator>>();
        var logger4 = new Mock<ILogger<TicketNotPresentTokenIntegrationValidator>>();
        var logger5 = new Mock<ILogger<TicketNotAJwtTokenIntegrationValidator>>();
        var logger6 = new Mock<ILogger<AsUriNotPresentValidator>>();
        var logger7 = new Mock<ILogger<AsUriNotAUrlValidator>>();

        // Create the validators
        var validators = new List<ITokenRequestValidator<TokenIntegrationRequestModel>>
        {
            new RqpNotPresentValidator(logger2.Object),
            new RqpNotAJwtValidator(logger3.Object),
            new TicketNotPresentTokenIntegrationValidator(logger4.Object),
            new TicketNotAJwtTokenIntegrationValidator(logger5.Object),
            new AsUriNotPresentValidator(logger6.Object),
            new AsUriNotAUrlValidator(logger7.Object)
        };

        // Return the ordered validators based on the Order property
        return validators.OrderBy(v => v.Order).ToList();
    }
}