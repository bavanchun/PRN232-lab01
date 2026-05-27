using Newtonsoft.Json;
using PRN232.Lab1.API.Common;

namespace PRN232.Lab1.API.Models.Response;

public class SubjectResponse
{
    public int SubjectId { get; set; }
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public int Credit { get; set; }

    [JsonProperty("_links")]
    public Dictionary<string, HalLink> Links { get; set; } = new();
}
