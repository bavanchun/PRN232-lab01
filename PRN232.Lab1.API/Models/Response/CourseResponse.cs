using Newtonsoft.Json;
using PRN232.Lab1.API.Common;

namespace PRN232.Lab1.API.Models.Response;

public class CourseResponse
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int SemesterId { get; set; }
    public string? SemesterName { get; set; }
    public SemesterResponse? Semester { get; set; }
    public List<EnrollmentResponse>? Enrollments { get; set; }

    [JsonProperty("_links")]
    public Dictionary<string, HalLink> Links { get; set; } = new();
}
