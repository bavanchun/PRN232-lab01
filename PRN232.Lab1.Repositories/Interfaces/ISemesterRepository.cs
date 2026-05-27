using PRN232.Lab1.Repositories.Entities;

namespace PRN232.Lab1.Repositories.Interfaces;

public interface ISemesterRepository : IGenericRepository<Semester, int>
{
    IQueryable<Semester> QueryWithCourses();
    IQueryable<Course> QueryCoursesBySemesterId(int semesterId);
}
