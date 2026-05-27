namespace PRN232.Lab1.Services.Models;

public class StudentBusiness
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public List<EnrollmentBusiness>? Enrollments { get; set; }
}
