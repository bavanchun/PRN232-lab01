using Microsoft.EntityFrameworkCore;
using PRN232.Lab1.Repositories.Entities;
using PRN232.Lab1.Repositories.Interfaces;

namespace PRN232.Lab1.Repositories.Implementations;

public class EnrollmentRepository : GenericRepository<Enrollment, int>, IEnrollmentRepository
{
    public EnrollmentRepository(LmsDbContext db) : base(db) { }

    public IQueryable<Enrollment> QueryWithStudentAndCourse() =>
        Db.Enrollments.AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course).ThenInclude(c => c.Semester);
}
