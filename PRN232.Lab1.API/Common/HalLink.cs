using Newtonsoft.Json;

namespace PRN232.Lab1.API.Common;

public class HalLink
{
    [JsonProperty("href")]
    public string Href { get; set; } = string.Empty;

    [JsonProperty("method")]
    public string Method { get; set; } = "GET";
}
