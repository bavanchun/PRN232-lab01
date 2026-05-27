namespace PRN232.Lab1.Services.Models;

public class SemesterBusiness
{
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<CourseBusiness>? Courses { get; set; }
}
