using Newtonsoft.Json;
using PRN232.Lab1.API.Common;

namespace PRN232.Lab1.API.Models.Response;

public class StudentResponse
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public List<EnrollmentResponse>? Enrollments { get; set; }

    [JsonProperty("_links")]
    public Dictionary<string, HalLink> Links { get; set; } = new();
}
