using Microsoft.EntityFrameworkCore;
using PRN232.Lab1.Repositories.Entities;
using PRN232.Lab1.Repositories.Interfaces;

namespace PRN232.Lab1.Repositories.Implementations;

public class StudentRepository : GenericRepository<Student, int>, IStudentRepository
{
    public StudentRepository(LmsDbContext db) : base(db) { }

    public IQueryable<Enrollment> QueryEnrollmentsByStudentId(int studentId) =>
        Db.Enrollments.AsNoTracking().Where(e => e.StudentId == studentId);
}
