using Microsoft.EntityFrameworkCore;
using PRN232.Lab1.Repositories.Entities;
using PRN232.Lab1.Repositories.Interfaces;

namespace PRN232.Lab1.Repositories.Implementations;

public class SemesterRepository : GenericRepository<Semester, int>, ISemesterRepository
{
    public SemesterRepository(LmsDbContext db) : base(db) { }

    public IQueryable<Semester> QueryWithCourses() =>
        Db.Semesters.AsNoTracking().Include(s => s.Courses);

    public IQueryable<Course> QueryCoursesBySemesterId(int semesterId) =>
        Db.Courses.AsNoTracking().Where(c => c.SemesterId == semesterId);
}
