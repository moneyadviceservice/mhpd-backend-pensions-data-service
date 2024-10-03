using System;
using MaPSCDAService.Utils;
using System.Security;
using Xunit;
using Xunit.Abstractions; // Update with the correct namespace for PKCEUtils

namespace MaPSCDAServiceUnitTests;

public class PkceGeneratorUnitTests
{
    
    private readonly PkceGenerator pKCEUtils;

    public PkceGeneratorUnitTests()
    {
        pKCEUtils = new PkceGenerator();
        
    }

    [Fact]
    public void GeneratePKCE_ReturnsValidVerifierAndChallenge()
    {
        // Arrange & Act
        (string codeVerifier, string codeChallenge) = pKCEUtils.GeneratePkce();

        // Assert
        Assert.False(string.IsNullOrEmpty(codeVerifier), "Code verifier should not be null or empty.");
        Assert.False(string.IsNullOrEmpty(codeChallenge), "Code challenge should not be null or empty.");

        Assert.True(codeVerifier.Length >= 43 && codeVerifier.Length <= 128, "Code verifier should be between 43 and 128 characters.");

        // Ensure the code verifier and code challenge only contain valid URL-safe characters
        Assert.Matches("^[a-zA-Z0-9-_]*$", codeVerifier);
        Assert.Matches("^[a-zA-Z0-9-_]*$", codeChallenge);
    }

    [Fact]
    public void GeneratePKCE_ProducesDifferentChallengeForDifferentVerifiers()
    {
        // Arrange & Act
        (string codeVerifier1, string codeChallenge1) = pKCEUtils.GeneratePkce();
        (string codeVerifier2, string codeChallenge2) = pKCEUtils.GeneratePkce();

        // Assert
        Assert.NotEqual(codeVerifier1, codeVerifier2);
        Assert.NotEqual(codeChallenge1, codeChallenge2);
    }

    [Fact]
    public void GeneratePKCE_ProducesSameChallengeForSameVerifier()
    {
        // Arrange & Act
        (string codeVerifier, string codeChallenge1) = pKCEUtils.GeneratePkce();
        string codeChallenge2;

        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            byte[] challengeBytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.ASCII.GetBytes(codeVerifier));
            codeChallenge2 = Convert.ToBase64String(challengeBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", ""); // Base64 URL encoding, without padding characters
        }

        // Assert
        Assert.Equal(codeChallenge1, codeChallenge2);
    }
}