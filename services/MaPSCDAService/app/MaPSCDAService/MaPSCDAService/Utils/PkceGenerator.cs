using System;
using System.Security.Cryptography;
using System.Text;

namespace MaPSCDAService.Utils;

public class PkceGenerator : IPkceGenerator
{
    // Generates a random code verifier and its corresponding code challenge as per RFC 7636 specifications.
    // https://www.rfc-editor.org/rfc/rfc7636.html#page-8
    public (string codeVerifier, string codeChallenge) GeneratePkce()
    {
        const int length = 96; // Maximum length is set so that the encoded size does not exceed 128 characters
        var randomBytes = new byte[length];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        // Base64 URL encoding, without padding characters (RFC 7636 specification)
        var codeVerifier = Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");

        // Generate code challenge using SHA-256 hash of the code verifier
        string codeChallenge;
        using (var sha256 = SHA256.Create())
        {
            byte[] challengeBytes = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
            codeChallenge = Convert.ToBase64String(challengeBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", ""); // Base64 URL encoding, without padding characters
        }

        return (codeVerifier: codeVerifier, codeChallenge: codeChallenge);
    }
}