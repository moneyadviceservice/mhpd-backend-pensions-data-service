using System.Text.Json.Serialization;

namespace MhpdCommon.Models.MHPDModels.JwkUri;

public class JwkUriResponseModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("keys")]
    public List<JwkKey> Keys { get; set; } = [];

    public static JwkUriResponseModel Default()
    {
        return new JwkUriResponseModel
        {
            Keys = [
                new JwkKey
                {
                    KeyType = JwkConstants.KeyType,
                    KeyId = JwkConstants.KeyId,
                    Modulus = JwkConstants.Modulus,
                    Exponent = JwkConstants.Exponent,
                    Algorithm = JwkConstants.Algorithm
                },
                new JwkKey
                {
                    KeyType = JwkConstants.KeyType,
                    KeyId = JwkConstants.ViewDataKeyId,
                    Modulus = JwkConstants.ViewDataModulus,
                    Exponent = JwkConstants.Exponent,
                    Algorithm = JwkConstants.Algorithm
                },
                new JwkKey
                {
                    KeyType = JwkConstants.KeyType,
                    KeyId = JwkConstants.MHPDKeyId,
                    Modulus = JwkConstants.MHPDModulus,
                    Exponent = JwkConstants.Exponent,
                    Algorithm = JwkConstants.Algorithm
                }
            ]
        };
    }
}