using System.Text.Json.Serialization;

namespace ProductService.Settings
{
    public class TokenConfig
    {
        [JsonPropertyName("secret")]
        public string Secret { get; set; }

        [JsonPropertyName("appSecret")]
        public string AppSecret { get; set; }

        [JsonPropertyName("issuer")]
        public string Issuer { get; set; }

        [JsonPropertyName("audience")]
        public string Audience { get; set; }

        [JsonPropertyName("accessExpiration")]
        public int AccessExpiration { get; set; }

        [JsonPropertyName("appAccessExpiration")]
        public int AppAccessExpiration { get; set; }

        [JsonPropertyName("authenticateScheme")]
        public string AuthenticateScheme { get; set; }
    }
}