using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.TokenValidation;
using MhpdCommon.Utils;
using Microsoft.Extensions.Logging;
using Moq;

namespace CDAServiceEmulatorUnitTests;

public static class Helper
{
    public const string ValidRedirectUri = "https://www.example.com/api/1";
    public const string GeneratedRsaPrivateKeyPem =
        "-----BEGIN RSA PRIVATE KEY-----\r\nMIIEpAIBAAKCAQEAu+NIp4+Hm5qJoEmQwfYm/+JTph1bqTow6bwTWGVLb0LihRQH\r\n2Z5im04zBY7JekJcY4t3xiCz4lbO5aAjPkWRSNMLCQtT3U/tE4ouQc3CXal/BKVV\r\nThzEx4uRYOlfwm7MBuP0ywmArqL/uzv+nXR6d1SbJen0aPVq5GI1+pZPD32XQrx1\r\nVSPRvwHdbJKlJKv0Am0l5JAJhlqasWcL39qXVdlUl7BSpQ0RsqI9vQDTHaHT4f0a\r\n9WaE/0Bqs0wU5lwGKCfrF3QQyPTfqiRBKJhS8OJWA5Umm9OmZyUUvfhX9TR2yi0a\r\nB91wZixUZ1moc0c5hDbgT3ApwpqMfmpmD/MB4QIDAQABAoIBAQCbDCpurBEaJWMh\r\nNNQSW9E/JEQnNt7nPbATkjLcpWqkvCs16pu3x+5Tfyq3kWdOTujy9Kq4g1AunbhK\r\n4eVzg/EqiY33vrNMVKKtl0Ao6WVV6YN6D/3fYfo5OUGVGcD+MHSJ0x+1VXgtpTEo\r\nD5BD21UcmGMX3ILnAm9dXHZy/grsGijjO6kUXjFUFfiw01hNncOebAKRver7FaK7\r\noeac0QO1Pld2b/IBw5SkaqnQ9+cU4+RA6R23GKhvj+aMMCVNQ+vNr5wZd1Po7+HB\r\n6QAckfCtkSHtaGGt2vg/Ry19qRjLFq/Z9P87r6gvAgE/b3nCWJEmsqcXjtarMUus\r\nHVZkvnSJAoGBAPnpqm7F5SQGide98GeLrw71JqIO/UC+vsNTO6juMo0/cLu1k4Ju\r\n6wHJjW8EYK23V15zPQU+zrpc4ns6ZftQcVDfpXZDHMfWxZCGsmLH7gFjCXGxzC+t\r\nFtXklx1Skf23nZ9KK2SMsvoPckBplWSw+rNf5e4a1QOlOI+PkHWtzG1fAoGBAMB2\r\n3F1VvFFnz/o2trja0mGvd7hlla2E6PrpLAdzUdRXcBns+WA3i6zdmdj+p7Na5ba+\r\nDL8ZAt3hPIQa+1sjfdmtOTNvYIJEooLUcC70REa1BeQ0DCDomYyNzeAR1pU4Qv8l\r\nLN0iteKLben9BllRASijhGQwPfaNSLwzIf2go5i/AoGAaY9bYALS8M6aNriR7QrB\r\nXM4MDXPLYSewqAxmLMrAK41abs8ZiYSUW2LpDLWKqJXCn7CJW8fVxj4po+dc4DRw\r\nSLrOxw89/uMm/A8JFlRgodFVUeLJ3nF8LciqU8ZmyAZg28GWZphPmPQhTDfM1IU0\r\nv8OH+XT3+Nw28dywJiTNLn0CgYAeYKRXdvjKQIBrFcexVZhvIqrax+3f/WJod/Uj\r\nF+iKg2KGNQkHTr0VA8UMouUFQguP1F9eqivxqWNL1pZlxCxQ9y5YF/Q7m2mrDKzI\r\nPHWqU1gitLRVXWEe9iLQgwBDfSXV76AtJxVeNHOcwvsFjeuI5oY26gZdq98XFVsA\r\nFSK9MQKBgQD5dsOKU+9iDVe9vPEArP61jI2YWlXHQ//pUnpP6nOvnLRwfiZ3uCWy\r\nUnPQm5hpHkQ3yWAZ7Qg9VmHdfeW37+s7vrecjopPfEAzkRBylqb6f8PvBpvbxBr3\r\nkIlKA7uY0Jma1Cr1Cz4q/BepAeVmXrgK9k6hJJQsPkTM/s4LpjZQ/g==\r\n-----END RSA PRIVATE KEY-----";
    
        // Helper method to create and order validators
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
}