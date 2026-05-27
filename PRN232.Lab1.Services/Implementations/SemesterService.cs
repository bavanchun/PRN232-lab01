using Microsoft.EntityFrameworkCore;
using PRN232.Lab1.Repositories.Entities;
using PRN232.Lab1.Repositories.Interfaces;
using PRN232.Lab1.Services.Helpers;
using PRN232.Lab1.Services.Interfaces;
using PRN232.Lab1.Services.Mapping;
using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.Services.Implementations;

public class SemesterService : ISemesterService
{
    private readonly ISemesterRepository _repo;
    private static readonly HashSet<string> SortFields = new(StringComparer.OrdinalIgnoreCase)
        { "semesterId", "semesterName", "startDate", "endDate" };

    public SemesterService(ISemesterRepository repo) { _repo = repo; }

    public async Task<BusinessResult<PagedResult<SemesterBusiness>>> ListAsync(QueryOptions opts)
    {
        try
        {
            var q = _repo.Query();
            q = QueryHelper.ApplySearch(q, opts.Search, s => s.SemesterName);
            q = QueryHelper.ApplySort(q, opts.Sort, SortFields, defaultSort: "semesterId");
            var includeCourses = QueryHelper.ShouldExpand(opts.Expand, "courses");
            if (includeCourses) q = q.Include(s => s.Courses);
            var paged = await QueryHelper.ToPagedAsync(q, opts.Page, opts.Size, e => e.ToBusiness(includeCourses));
            return BusinessResult<PagedResult<SemesterBusiness>>.Ok(paged);
        }
        catch (ArgumentException ex) { return BusinessResult<PagedResult<SemesterBusiness>>.Fail(ex.Message); }
    }

    public async Task<BusinessResult<SemesterBusiness>> GetByIdAsync(int id, string? expand = null)
    {
        var includeCourses = QueryHelper.ShouldExpand(expand, "courses");
        var entity = includeCourses
            ? await _repo.QueryWithCourses().FirstOrDefaultAsync(s => s.SemesterId == id)
            : await _repo.GetByIdAsync(id);
        return entity is null
            ? BusinessResult<SemesterBusiness>.NotFound($"Semester {id} not found")
            : BusinessResult<SemesterBusiness>.Ok(entity.ToBusiness(includeCourses));
    }

    public async Task<BusinessResult<SemesterBusiness>> CreateAsync(SemesterBusiness input)
    {
        var entity = input.ToEntity();
        await _repo.AddAsync(entity);
        return BusinessResult<SemesterBusiness>.Ok(entity.ToBusiness(), "Semester created");
    }

    public async Task<BusinessResult<SemesterBusiness>> UpdateAsync(int id, SemesterBusiness input)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null) return BusinessResult<SemesterBusiness>.NotFound($"Semester {id} not found");
        input.ApplyTo(entity);
        await _repo.UpdateAsync(entity);
        return BusinessResult<SemesterBusiness>.Ok(entity.ToBusiness(), "Semester updated");
    }

    public async Task<BusinessResult<bool>> DeleteAsync(int id)
    {
        var ok = await _repo.DeleteAsync(id);
        return ok
            ? BusinessResult<bool>.Ok(true, "Semester deleted")
            : BusinessResult<bool>.NotFound($"Semester {id} not found");
    }

    public async Task<BusinessResult<bool>> ExistsAsync(int id)
        => BusinessResult<bool>.Ok(await _repo.ExistsAsync(id));

    public async Task<BusinessResult<PagedResult<CourseBusiness>>> ListCoursesAsync(int semesterId, QueryOptions opts)
    {
        if (!await _repo.ExistsAsync(semesterId))
            return BusinessResult<PagedResult<CourseBusiness>>.NotFound($"Semester {semesterId} not found");
        try
        {
            var q = _repo.QueryCoursesBySemesterId(semesterId);
            q = QueryHelper.ApplySearch(q, opts.Search, c => c.CourseName);
            q = QueryHelper.ApplySort(q, opts.Sort,
                new(StringComparer.OrdinalIgnoreCase) { "courseId", "courseName", "semesterId" },
                defaultSort: "courseId");
            var expandSemester = QueryHelper.ShouldExpand(opts.Expand, "semester");
            if (expandSemester) q = q.Include(c => c.Semester);
            var paged = await QueryHelper.ToPagedAsync(q, opts.Page, opts.Size,
                e => e.ToBusiness(expandSemester));
            return BusinessResult<PagedResult<CourseBusiness>>.Ok(paged);
        }
        catch (ArgumentException ex) { return BusinessResult<PagedResult<CourseBusiness>>.Fail(ex.Message); }
    }
}
