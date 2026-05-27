namespace PRN232.Lab1.API.Models.Request;

public class CreateCourseRequest
{
    public string CourseName { get; set; } = string.Empty;
    public int SemesterId { get; set; }
}

public class UpdateCourseRequest
{
    public string CourseName { get; set; } = string.Empty;
    public int SemesterId { get; set; }
}
