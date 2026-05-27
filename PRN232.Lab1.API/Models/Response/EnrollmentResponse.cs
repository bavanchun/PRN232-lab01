using Newtonsoft.Json;
using PRN232.Lab1.API.Common;

namespace PRN232.Lab1.API.Models.Response;

public class EnrollmentResponse
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public string? StudentName { get; set; }
    public int CourseId { get; set; }
    public string? CourseName { get; set; }
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public StudentResponse? Student { get; set; }
    public CourseResponse? Course { get; set; }

    [JsonProperty("_links")]
    public Dictionary<string, HalLink> Links { get; set; } = new();
}
