using Newtonsoft.Json;
using System.Collections.Generic;

namespace ApiGateway.Settings
{
    [JsonObject("CorsConfig")]
    public class CorsConfig
    {
        [JsonProperty("Urls")]
        public List<string> Urls { get; set; }
    }
}
