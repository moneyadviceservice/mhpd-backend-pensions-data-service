namespace MaPSCDAService.Utils;

public interface IPkceGenerator
{
    (string codeVerifier, string codeChallenge) GeneratePkce();
}
