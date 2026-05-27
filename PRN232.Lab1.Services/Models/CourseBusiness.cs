namespace PRN232.Lab1.Services.Models;

public class CourseBusiness
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int SemesterId { get; set; }
    public string? SemesterName { get; set; }
    public SemesterBusiness? Semester { get; set; }
    public List<EnrollmentBusiness>? Enrollments { get; set; }
}
