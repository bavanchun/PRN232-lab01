using PRN232.Lab1.Repositories.Entities;
using PRN232.Lab1.Repositories.Interfaces;

namespace PRN232.Lab1.Repositories.Implementations;

public class SubjectRepository : GenericRepository<Subject, int>, ISubjectRepository
{
    public SubjectRepository(LmsDbContext db) : base(db) { }
}
