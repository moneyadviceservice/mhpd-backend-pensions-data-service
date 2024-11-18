using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CDAServiceEmulator.Models.JwkUri;

public class JwkUriResponseModel
{
    [JsonPropertyName("keys")]
    public List<JwkKey> Keys { get; set; } = new()
    {
        new JwkKey
        {
            KeyType = JwkConstants.KeyType,
            KeyId = JwkConstants.KeyId,
            Modulus = JwkConstants.Modulus,
            Exponent = JwkConstants.Exponent
        }
    };
}