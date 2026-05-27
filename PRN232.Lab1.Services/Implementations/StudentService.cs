using Microsoft.EntityFrameworkCore;
using PRN232.Lab1.Repositories.Interfaces;
using PRN232.Lab1.Services.Helpers;
using PRN232.Lab1.Services.Interfaces;
using PRN232.Lab1.Services.Mapping;
using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.Services.Implementations;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _repo;
    private static readonly HashSet<string> SortFields = new(StringComparer.OrdinalIgnoreCase)
        { "studentId", "fullName", "email", "dateOfBirth" };

    public StudentService(IStudentRepository repo) { _repo = repo; }

    public async Task<BusinessResult<PagedResult<StudentBusiness>>> ListAsync(QueryOptions opts)
    {
        try
        {
            var q = _repo.Query();
            q = QueryHelper.ApplySearch(q, opts.Search, s => s.FullName, s => s.Email);
            q = QueryHelper.ApplySort(q, opts.Sort, SortFields, defaultSort: "studentId");
            var expandEnrollments = QueryHelper.ShouldExpand(opts.Expand, "enrollments");
            if (expandEnrollments) q = q.Include(s => s.Enrollments).ThenInclude(e => e.Course);
            var paged = await QueryHelper.ToPagedAsync(q, opts.Page, opts.Size,
                e => e.ToBusiness(expandEnrollments));
            return BusinessResult<PagedResult<StudentBusiness>>.Ok(paged);
        }
        catch (ArgumentException ex) { return BusinessResult<PagedResult<StudentBusiness>>.Fail(ex.Message); }
    }

    public async Task<BusinessResult<StudentBusiness>> GetByIdAsync(int id, string? expand = null)
    {
        var expandEnrollments = QueryHelper.ShouldExpand(expand, "enrollments");
        var q = _repo.Query();
        if (expandEnrollments) q = q.Include(s => s.Enrollments).ThenInclude(e => e.Course);
        var entity = await q.FirstOrDefaultAsync(s => s.StudentId == id);
        return entity is null
            ? BusinessResult<StudentBusiness>.NotFound($"Student {id} not found")
            : BusinessResult<StudentBusiness>.Ok(entity.ToBusiness(expandEnrollments));
    }

    public async Task<BusinessResult<StudentBusiness>> CreateAsync(StudentBusiness input)
    {
        var entity = input.ToEntity();
        try
        {
            await _repo.AddAsync(entity);
            return BusinessResult<StudentBusiness>.Ok(entity.ToBusiness(), "Student created");
        }
        catch (DbUpdateException ex)
        {
            return BusinessResult<StudentBusiness>.Fail($"Create failed: {ex.InnerException?.Message ?? ex.Message}");
        }
    }

    public async Task<BusinessResult<StudentBusiness>> UpdateAsync(int id, StudentBusiness input)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null) return BusinessResult<StudentBusiness>.NotFound($"Student {id} not found");
        input.ApplyTo(entity);
        try
        {
            await _repo.UpdateAsync(entity);
            return BusinessResult<StudentBusiness>.Ok(entity.ToBusiness(), "Student updated");
        }
        catch (DbUpdateException ex)
        {
            return BusinessResult<StudentBusiness>.Fail($"Update failed: {ex.InnerException?.Message ?? ex.Message}");
        }
    }

    public async Task<BusinessResult<bool>> DeleteAsync(int id)
    {
        var ok = await _repo.DeleteAsync(id);
        return ok
            ? BusinessResult<bool>.Ok(true, "Student deleted")
            : BusinessResult<bool>.NotFound($"Student {id} not found");
    }

    public async Task<BusinessResult<bool>> ExistsAsync(int id)
        => BusinessResult<bool>.Ok(await _repo.ExistsAsync(id));

    public async Task<BusinessResult<PagedResult<EnrollmentBusiness>>> ListEnrollmentsAsync(int studentId, QueryOptions opts)
    {
        if (!await _repo.ExistsAsync(studentId))
            return BusinessResult<PagedResult<EnrollmentBusiness>>.NotFound($"Student {studentId} not found");
        try
        {
            IQueryable<Repositories.Entities.Enrollment> q = _repo.QueryEnrollmentsByStudentId(studentId)
                .Include(e => e.Student).Include(e => e.Course);
            q = QueryHelper.ApplySearch(q, opts.Search, e => e.Status);
            q = QueryHelper.ApplySort(q, opts.Sort,
                new(StringComparer.OrdinalIgnoreCase) { "enrollmentId", "studentId", "courseId", "enrollDate", "status" },
                defaultSort: "-enrollDate");
            var expandStudent = QueryHelper.ShouldExpand(opts.Expand, "student");
            var expandCourse = QueryHelper.ShouldExpand(opts.Expand, "course");
            var paged = await QueryHelper.ToPagedAsync(q, opts.Page, opts.Size,
                e => e.ToBusiness(expandStudent, expandCourse));
            return BusinessResult<PagedResult<EnrollmentBusiness>>.Ok(paged);
        }
        catch (ArgumentException ex) { return BusinessResult<PagedResult<EnrollmentBusiness>>.Fail(ex.Message); }
    }
}
