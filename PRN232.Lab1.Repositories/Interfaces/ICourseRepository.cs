using PRN232.Lab1.Repositories.Entities;

namespace PRN232.Lab1.Repositories.Interfaces;

public interface ICourseRepository : IGenericRepository<Course, int>
{
    IQueryable<Course> QueryWithSemester();
    IQueryable<Enrollment> QueryEnrollmentsByCourseId(int courseId);
}
