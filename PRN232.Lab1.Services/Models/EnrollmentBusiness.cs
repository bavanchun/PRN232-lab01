namespace PRN232.Lab1.Services.Models;

public class EnrollmentBusiness
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public string? StudentName { get; set; }
    public int CourseId { get; set; }
    public string? CourseName { get; set; }
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public StudentBusiness? Student { get; set; }
    public CourseBusiness? Course { get; set; }
}
