using Microsoft.EntityFrameworkCore;
using PRN232.Lab1.Repositories.Interfaces;
using PRN232.Lab1.Services.Helpers;
using PRN232.Lab1.Services.Interfaces;
using PRN232.Lab1.Services.Mapping;
using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.Services.Implementations;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _repo;
    private readonly IStudentRepository _studentRepo;
    private readonly ICourseRepository _courseRepo;
    private static readonly HashSet<string> SortFields = new(StringComparer.OrdinalIgnoreCase)
        { "enrollmentId", "studentId", "courseId", "enrollDate", "status" };

    public EnrollmentService(IEnrollmentRepository repo, IStudentRepository studentRepo, ICourseRepository courseRepo)
    {
        _repo = repo; _studentRepo = studentRepo; _courseRepo = courseRepo;
    }

    public async Task<BusinessResult<PagedResult<EnrollmentBusiness>>> ListAsync(QueryOptions opts)
    {
        try
        {
            IQueryable<Repositories.Entities.Enrollment> q = _repo.Query()
                .Include(e => e.Student).Include(e => e.Course);
            q = QueryHelper.ApplySearch(q, opts.Search, e => e.Status);
            q = QueryHelper.ApplySort(q, opts.Sort, SortFields, defaultSort: "enrollmentId");
            var expandStudent = QueryHelper.ShouldExpand(opts.Expand, "student");
            var expandCourse = QueryHelper.ShouldExpand(opts.Expand, "course");
            if (expandCourse) q = q.Include(e => e.Course).ThenInclude(c => c.Semester);
            var paged = await QueryHelper.ToPagedAsync(q, opts.Page, opts.Size,
                e => e.ToBusiness(expandStudent, expandCourse));
            return BusinessResult<PagedResult<EnrollmentBusiness>>.Ok(paged);
        }
        catch (ArgumentException ex) { return BusinessResult<PagedResult<EnrollmentBusiness>>.Fail(ex.Message); }
    }

    public async Task<BusinessResult<EnrollmentBusiness>> GetByIdAsync(int id, string? expand = null)
    {
        var expandStudent = QueryHelper.ShouldExpand(expand, "student");
        var expandCourse = QueryHelper.ShouldExpand(expand, "course");
        IQueryable<Repositories.Entities.Enrollment> q = _repo.Query()
            .Include(e => e.Student).Include(e => e.Course);
        if (expandCourse) q = q.Include(e => e.Course).ThenInclude(c => c.Semester);
        var entity = await q.FirstOrDefaultAsync(e => e.EnrollmentId == id);
        return entity is null
            ? BusinessResult<EnrollmentBusiness>.NotFound($"Enrollment {id} not found")
            : BusinessResult<EnrollmentBusiness>.Ok(entity.ToBusiness(expandStudent, expandCourse));
    }

    public async Task<BusinessResult<EnrollmentBusiness>> CreateAsync(EnrollmentBusiness input)
    {
        if (!await _studentRepo.ExistsAsync(input.StudentId))
            return BusinessResult<EnrollmentBusiness>.Fail($"Student {input.StudentId} does not exist");
        if (!await _courseRepo.ExistsAsync(input.CourseId))
            return BusinessResult<EnrollmentBusiness>.Fail($"Course {input.CourseId} does not exist");
        var entity = input.ToEntity();
        await _repo.AddAsync(entity);
        return BusinessResult<EnrollmentBusiness>.Ok(entity.ToBusiness(), "Enrollment created");
    }

    public async Task<BusinessResult<EnrollmentBusiness>> UpdateAsync(int id, EnrollmentBusiness input)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null) return BusinessResult<EnrollmentBusiness>.NotFound($"Enrollment {id} not found");
        if (!await _studentRepo.ExistsAsync(input.StudentId))
            return BusinessResult<EnrollmentBusiness>.Fail($"Student {input.StudentId} does not exist");
        if (!await _courseRepo.ExistsAsync(input.CourseId))
            return BusinessResult<EnrollmentBusiness>.Fail($"Course {input.CourseId} does not exist");
        input.ApplyTo(entity);
        await _repo.UpdateAsync(entity);
        return BusinessResult<EnrollmentBusiness>.Ok(entity.ToBusiness(), "Enrollment updated");
    }

    public async Task<BusinessResult<bool>> DeleteAsync(int id)
    {
        var ok = await _repo.DeleteAsync(id);
        return ok
            ? BusinessResult<bool>.Ok(true, "Enrollment deleted")
            : BusinessResult<bool>.NotFound($"Enrollment {id} not found");
    }

    public async Task<BusinessResult<bool>> ExistsAsync(int id)
        => BusinessResult<bool>.Ok(await _repo.ExistsAsync(id));
}
