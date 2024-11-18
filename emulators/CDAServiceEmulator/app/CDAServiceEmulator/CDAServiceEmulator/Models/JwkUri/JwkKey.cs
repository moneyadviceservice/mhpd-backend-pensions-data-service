using System.Text.Json.Serialization;

namespace CDAServiceEmulator.Models.JwkUri;

public class JwkKey
{
    [JsonPropertyName("kty")]
    public string KeyType { get; set; } = JwkConstants.KeyType;

    [JsonPropertyName("kid")]
    public string KeyId { get; set; } = JwkConstants.KeyId;

    [JsonPropertyName("n")]
    public string Modulus { get; set; } = JwkConstants.Modulus;

    [JsonPropertyName("e")]
    public string Exponent { get; set; } = JwkConstants.Exponent;
}