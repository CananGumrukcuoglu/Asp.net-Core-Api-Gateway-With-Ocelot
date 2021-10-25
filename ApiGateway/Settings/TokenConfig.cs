using Newtonsoft.Json;

namespace ApiGateway.Settings
{
    [JsonObject("TokenConfig")]
    public class TokenConfig
    {
        [JsonProperty("secret")]
        public string Secret { get; set; }

        [JsonProperty("issuer")]
        public string Issuer { get; set; }

        [JsonProperty("audience")]
        public string Audience { get; set; }

        [JsonProperty("accessExpiration")]
        public int AccessExpiration { get; set; }

        [JsonProperty("authenticateScheme")]
        public string AuthenticateScheme { get; set; }
    }
}
