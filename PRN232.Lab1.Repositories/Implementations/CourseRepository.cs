using Microsoft.EntityFrameworkCore;
using PRN232.Lab1.Repositories.Entities;
using PRN232.Lab1.Repositories.Interfaces;

namespace PRN232.Lab1.Repositories.Implementations;

public class CourseRepository : GenericRepository<Course, int>, ICourseRepository
{
    public CourseRepository(LmsDbContext db) : base(db) { }

    public IQueryable<Course> QueryWithSemester() =>
        Db.Courses.AsNoTracking().Include(c => c.Semester);

    public IQueryable<Enrollment> QueryEnrollmentsByCourseId(int courseId) =>
        Db.Enrollments.AsNoTracking().Where(e => e.CourseId == courseId);
}
