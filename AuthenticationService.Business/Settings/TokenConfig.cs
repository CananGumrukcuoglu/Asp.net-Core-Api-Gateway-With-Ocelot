using Newtonsoft.Json;

namespace AuthenticationService.Business.Settings
{
    public class TokenConfig
    {
        [JsonProperty("secret")]
        public string Secret { get; set; }

        [JsonProperty("appSecret")]
        public string AppSecret { get; set; }

        [JsonProperty("issuer")]
        public string Issuer { get; set; }

        [JsonProperty("audience")]
        public string Audience { get; set; }

        [JsonProperty("accessExpiration")]
        public int AccessExpiration { get; set; }

        [JsonProperty("appAccessExpiration")]
        public int AppAccessExpiration { get; set; }

        [JsonProperty("authenticateScheme")]
        public string AuthenticateScheme { get; set; }
        [JsonProperty("iSalesClaimName")]
        public string ISalesClaimName { get; set; }
    }
}
