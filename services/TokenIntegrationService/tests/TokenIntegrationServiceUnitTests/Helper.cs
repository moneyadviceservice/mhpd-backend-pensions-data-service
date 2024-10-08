using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace TokenIntegrationServiceUnitTests;

public static class Helper
{
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