using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace PensionsDataServiceUnitTests;

public static class Helper
{
        // Helper method to create and order validators
    public static List<ITokenRequestValidator<PensionsDataRequestModel>> GetOrderedValidators()
    {
        // Mock loggers for each validator
        var logger2 = new Mock<ILogger<AuthorisationCodeNotPresentValidationPensionsData>>();
        var logger3 = new Mock<ILogger<AuthorisationCodeInvalidFormatValidationPensionsData>>();
        var logger4 = new Mock<ILogger<RedirectUriNotPresentValidationPensionsData>>();
        var logger5 = new Mock<ILogger<RedirectUriNotValidUrlValidationPensionsData>>();
        var logger6 = new Mock<ILogger<CodeVerifierNotPresentValidationPensionsData>>();
        var logger7 = new Mock<ILogger<CodeVerifierNotBase64StringPensionsData>>();

        // Create the validators
        var validators = new List<ITokenRequestValidator<PensionsDataRequestModel>>
        {
            new AuthorisationCodeNotPresentValidationPensionsData(logger2.Object),
            new AuthorisationCodeInvalidFormatValidationPensionsData(logger3.Object),
            new RedirectUriNotPresentValidationPensionsData(logger4.Object),
            new RedirectUriNotValidUrlValidationPensionsData(logger5.Object),
            new CodeVerifierNotPresentValidationPensionsData(logger6.Object),
            new CodeVerifierNotBase64StringPensionsData(logger7.Object),
        };

        // Return the ordered validators based on the Order property
        return validators.OrderBy(v => v.Order).ToList();
    }
}