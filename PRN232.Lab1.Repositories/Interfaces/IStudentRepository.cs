using PRN232.Lab1.Repositories.Entities;

namespace PRN232.Lab1.Repositories.Interfaces;

public interface IStudentRepository : IGenericRepository<Student, int>
{
    IQueryable<Enrollment> QueryEnrollmentsByStudentId(int studentId);
}
