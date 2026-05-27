using Newtonsoft.Json;
using PRN232.Lab1.API.Common;

namespace PRN232.Lab1.API.Models.Response;

public class SemesterResponse
{
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<CourseResponse>? Courses { get; set; }

    [JsonProperty("_links")]
    public Dictionary<string, HalLink> Links { get; set; } = new();
}
