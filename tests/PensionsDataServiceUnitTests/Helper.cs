using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using MhpdCommon.Utils;
using Microsoft.Extensions.Logging;
using Moq;

namespace PensionsDataServiceUnitTests;

public static class Helper
{
    public const string ValidRedirectUri = "https://www.example.com/api/1";
    
    // Helper method to create and order validators
    public static List<ITokenRequestValidator<PensionsDataRequestModel>> GetOrderedValidators()
    {
        // Mock loggers for each validator
        var logger2 = new Mock<ILogger<AuthorisationCodeNotPresentValidationPensionsData>>();
        var logger3 = new Mock<ILogger<AuthorisationCodeInvalidFormatValidationPensionsData>>();
        var logger4 = new Mock<ILogger<RedirectUrlNotPresentValidationPensionsData>>();
        var logger5 = new Mock<ILogger<RedirectUrlNotValidUrlValidationPensionsData>>();
        var logger6 = new Mock<ILogger<CodeVerifierNotPresentValidationPensionsData>>();
        var logger7 = new Mock<ILogger<CodeVerifierNotBase64StringPensionsData>>();
        var logger8 = new Mock<ILogger<ClientIdNotPresentValidationPensionData>>();
        var logger9 = new Mock<ILogger<ClientIdInvalidFormatValidationPensionData>>();
        var logger10 = new Mock<ILogger<ClientSecretNotPresentValidationPensionData>>();
        var logger11 = new Mock<ILogger<ClientSecretNotGuidValidationPensionData>>();
        
        // Mock Id validators
        var idValidator = new Mock<IdValidator>();

        // Create the validators
        var validators = new List<ITokenRequestValidator<PensionsDataRequestModel>>
        {
            new AuthorisationCodeNotPresentValidationPensionsData(logger2.Object),
            new AuthorisationCodeInvalidFormatValidationPensionsData(logger3.Object, new IdValidator()),
            new RedirectUrlNotPresentValidationPensionsData(logger4.Object),
            new RedirectUrlNotValidUrlValidationPensionsData(logger5.Object),
            new CodeVerifierNotPresentValidationPensionsData(logger6.Object),
            new CodeVerifierNotBase64StringPensionsData(logger7.Object),
            new ClientIdNotPresentValidationPensionData(logger8.Object),
            new ClientIdInvalidFormatValidationPensionData(logger9.Object, new IdValidator()),
            new ClientSecretNotPresentValidationPensionData(logger10.Object),
            new ClientSecretNotGuidValidationPensionData(logger11.Object, idValidator.Object),
        };

        // Return the ordered validators based on the Order property
        return validators.OrderBy(v => v.Order).ToList();
    }
}