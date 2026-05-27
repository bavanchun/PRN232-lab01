using PRN232.Lab1.API.Common;
using PRN232.Lab1.API.Models.Response;
using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.API.Mapping;

public class ResponseMappers
{
    private readonly LinkBuilder _links;

    public ResponseMappers(LinkBuilder links) { _links = links; }

    public SemesterResponse Map(SemesterBusiness b)
    {
        var r = new SemesterResponse
        {
            SemesterId = b.SemesterId,
            SemesterName = b.SemesterName,
            StartDate = b.StartDate,
            EndDate = b.EndDate,
            Courses = b.Courses?.Select(Map).ToList()
        };
        r.Links = _links.ForItem("semesters", new { id = b.SemesterId }, new[]
        {
            ("courses", "GET", "semesters.Courses", (object)new { id = b.SemesterId })
        });
        return r;
    }

    public CourseResponse Map(CourseBusiness b)
    {
        var r = new CourseResponse
        {
            CourseId = b.CourseId,
            CourseName = b.CourseName,
            SemesterId = b.SemesterId,
            SemesterName = b.SemesterName,
            Semester = b.Semester != null ? Map(b.Semester) : null,
            Enrollments = b.Enrollments?.Select(Map).ToList()
        };
        r.Links = _links.ForItem("courses", new { id = b.CourseId }, new[]
        {
            ("enrollments", "GET", "courses.Enrollments", (object)new { id = b.CourseId }),
            ("semester",    "GET", "semesters.GetById",   (object)new { id = b.SemesterId })
        });
        return r;
    }

    public SubjectResponse Map(SubjectBusiness b)
    {
        var r = new SubjectResponse
        {
            SubjectId = b.SubjectId,
            SubjectCode = b.SubjectCode,
            SubjectName = b.SubjectName,
            Credit = b.Credit
        };
        r.Links = _links.ForItem("subjects", new { id = b.SubjectId });
        return r;
    }

    public StudentResponse Map(StudentBusiness b)
    {
        var r = new StudentResponse
        {
            StudentId = b.StudentId,
            FullName = b.FullName,
            Email = b.Email,
            DateOfBirth = b.DateOfBirth,
            Enrollments = b.Enrollments?.Select(Map).ToList()
        };
        r.Links = _links.ForItem("students", new { id = b.StudentId }, new[]
        {
            ("enrollments", "GET", "students.Enrollments", (object)new { id = b.StudentId })
        });
        return r;
    }

    public EnrollmentResponse Map(EnrollmentBusiness b)
    {
        var r = new EnrollmentResponse
        {
            EnrollmentId = b.EnrollmentId,
            StudentId = b.StudentId,
            StudentName = b.StudentName,
            CourseId = b.CourseId,
            CourseName = b.CourseName,
            EnrollDate = b.EnrollDate,
            Status = b.Status,
            Student = b.Student != null ? Map(b.Student) : null,
            Course = b.Course != null ? Map(b.Course) : null
        };
        r.Links = _links.ForItem("enrollments", new { id = b.EnrollmentId }, new[]
        {
            ("student", "GET", "students.GetById", (object)new { id = b.StudentId }),
            ("course",  "GET", "courses.GetById",  (object)new { id = b.CourseId })
        });
        return r;
    }
}
