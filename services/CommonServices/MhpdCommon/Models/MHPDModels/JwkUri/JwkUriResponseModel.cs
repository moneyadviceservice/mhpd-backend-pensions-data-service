using System.Text.Json.Serialization;

namespace MhpdCommon.Models.MHPDModels.JwkUri;

public class JwkUriResponseModel
{
    [JsonPropertyName("keys")]
    public List<JwkKey> Keys { get; set; } =
    [
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
    ];
}