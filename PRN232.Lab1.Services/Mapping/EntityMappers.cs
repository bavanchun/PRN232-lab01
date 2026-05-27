using PRN232.Lab1.Repositories.Entities;
using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.Services.Mapping;

public static class EntityMappers
{
    // ─── Semester ────────────────────────────────────────────────
    public static SemesterBusiness ToBusiness(this Semester e, bool includeCourses = false) => new()
    {
        SemesterId = e.SemesterId,
        SemesterName = e.SemesterName,
        StartDate = e.StartDate,
        EndDate = e.EndDate,
        Courses = includeCourses && e.Courses?.Count > 0
            ? e.Courses.Select(c => c.ToBusiness()).ToList()
            : null
    };

    public static Semester ToEntity(this SemesterBusiness b) => new()
    {
        SemesterId = b.SemesterId,
        SemesterName = b.SemesterName,
        StartDate = b.StartDate,
        EndDate = b.EndDate
    };

    public static void ApplyTo(this SemesterBusiness b, Semester e)
    {
        e.SemesterName = b.SemesterName;
        e.StartDate = b.StartDate;
        e.EndDate = b.EndDate;
    }

    // ─── Course ──────────────────────────────────────────────────
    public static CourseBusiness ToBusiness(this Course e, bool expandSemester = false, bool expandEnrollments = false) => new()
    {
        CourseId = e.CourseId,
        CourseName = e.CourseName,
        SemesterId = e.SemesterId,
        SemesterName = e.Semester?.SemesterName,
        Semester = expandSemester && e.Semester != null ? e.Semester.ToBusiness() : null,
        Enrollments = expandEnrollments && e.Enrollments?.Count > 0
            ? e.Enrollments.Select(en => en.ToBusiness()).ToList()
            : null
    };

    public static Course ToEntity(this CourseBusiness b) => new()
    {
        CourseId = b.CourseId,
        CourseName = b.CourseName,
        SemesterId = b.SemesterId
    };

    public static void ApplyTo(this CourseBusiness b, Course e)
    {
        e.CourseName = b.CourseName;
        e.SemesterId = b.SemesterId;
    }

    // ─── Subject ─────────────────────────────────────────────────
    public static SubjectBusiness ToBusiness(this Subject e) => new()
    {
        SubjectId = e.SubjectId,
        SubjectCode = e.SubjectCode,
        SubjectName = e.SubjectName,
        Credit = e.Credit
    };

    public static Subject ToEntity(this SubjectBusiness b) => new()
    {
        SubjectId = b.SubjectId,
        SubjectCode = b.SubjectCode,
        SubjectName = b.SubjectName,
        Credit = b.Credit
    };

    public static void ApplyTo(this SubjectBusiness b, Subject e)
    {
        e.SubjectCode = b.SubjectCode;
        e.SubjectName = b.SubjectName;
        e.Credit = b.Credit;
    }

    // ─── Student ─────────────────────────────────────────────────
    public static StudentBusiness ToBusiness(this Student e, bool expandEnrollments = false) => new()
    {
        StudentId = e.StudentId,
        FullName = e.FullName,
        Email = e.Email,
        DateOfBirth = e.DateOfBirth,
        Enrollments = expandEnrollments && e.Enrollments?.Count > 0
            ? e.Enrollments.Select(en => en.ToBusiness()).ToList()
            : null
    };

    public static Student ToEntity(this StudentBusiness b) => new()
    {
        StudentId = b.StudentId,
        FullName = b.FullName,
        Email = b.Email,
        DateOfBirth = b.DateOfBirth
    };

    public static void ApplyTo(this StudentBusiness b, Student e)
    {
        e.FullName = b.FullName;
        e.Email = b.Email;
        e.DateOfBirth = b.DateOfBirth;
    }

    // ─── Enrollment ──────────────────────────────────────────────
    public static EnrollmentBusiness ToBusiness(this Enrollment e, bool expandStudent = false, bool expandCourse = false) => new()
    {
        EnrollmentId = e.EnrollmentId,
        StudentId = e.StudentId,
        StudentName = e.Student?.FullName,
        CourseId = e.CourseId,
        CourseName = e.Course?.CourseName,
        EnrollDate = e.EnrollDate,
        Status = e.Status,
        Student = expandStudent && e.Student != null ? e.Student.ToBusiness() : null,
        Course = expandCourse && e.Course != null ? e.Course.ToBusiness(expandSemester: true) : null
    };

    public static Enrollment ToEntity(this EnrollmentBusiness b) => new()
    {
        EnrollmentId = b.EnrollmentId,
        StudentId = b.StudentId,
        CourseId = b.CourseId,
        EnrollDate = b.EnrollDate,
        Status = b.Status
    };

    public static void ApplyTo(this EnrollmentBusiness b, Enrollment e)
    {
        e.StudentId = b.StudentId;
        e.CourseId = b.CourseId;
        e.EnrollDate = b.EnrollDate;
        e.Status = b.Status;
    }
}
