using Microsoft.EntityFrameworkCore;
using PRN232.Lab1.Repositories.Entities;
using PRN232.Lab1.Repositories.Interfaces;
using PRN232.Lab1.Services.Helpers;
using PRN232.Lab1.Services.Interfaces;
using PRN232.Lab1.Services.Mapping;
using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.Services.Implementations;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _repo;
    private readonly ISemesterRepository _semesterRepo;
    private readonly IStudentRepository _studentRepo;
    private static readonly HashSet<string> SortFields = new(StringComparer.OrdinalIgnoreCase)
        { "courseId", "courseName", "semesterId" };

    public CourseService(ICourseRepository repo, ISemesterRepository semesterRepo, IStudentRepository studentRepo)
    {
        _repo = repo; _semesterRepo = semesterRepo; _studentRepo = studentRepo;
    }

    public async Task<BusinessResult<PagedResult<CourseBusiness>>> ListAsync(QueryOptions opts)
    {
        try
        {
            var q = _repo.Query();
            q = QueryHelper.ApplySearch(q, opts.Search, c => c.CourseName);
            q = QueryHelper.ApplySort(q, opts.Sort, SortFields, defaultSort: "courseId");
            var expandSemester = QueryHelper.ShouldExpand(opts.Expand, "semester");
            var expandEnrollments = QueryHelper.ShouldExpand(opts.Expand, "enrollments");
            if (expandSemester) q = q.Include(c => c.Semester);
            if (expandEnrollments) q = q.Include(c => c.Enrollments);
            var paged = await QueryHelper.ToPagedAsync(q, opts.Page, opts.Size,
                e => e.ToBusiness(expandSemester, expandEnrollments));
            return BusinessResult<PagedResult<CourseBusiness>>.Ok(paged);
        }
        catch (ArgumentException ex) { return BusinessResult<PagedResult<CourseBusiness>>.Fail(ex.Message); }
    }

    public async Task<BusinessResult<CourseBusiness>> GetByIdAsync(int id, string? expand = null)
    {
        var expandSemester = QueryHelper.ShouldExpand(expand, "semester");
        var expandEnrollments = QueryHelper.ShouldExpand(expand, "enrollments");
        var q = _repo.Query();
        if (expandSemester) q = q.Include(c => c.Semester);
        if (expandEnrollments) q = q.Include(c => c.Enrollments).ThenInclude(en => en.Student);
        var entity = await q.FirstOrDefaultAsync(c => c.CourseId == id);
        return entity is null
            ? BusinessResult<CourseBusiness>.NotFound($"Course {id} not found")
            : BusinessResult<CourseBusiness>.Ok(entity.ToBusiness(expandSemester, expandEnrollments));
    }

    public async Task<BusinessResult<CourseBusiness>> CreateAsync(CourseBusiness input)
    {
        if (!await _semesterRepo.ExistsAsync(input.SemesterId))
            return BusinessResult<CourseBusiness>.Fail($"Semester {input.SemesterId} does not exist");
        var entity = input.ToEntity();
        await _repo.AddAsync(entity);
        return BusinessResult<CourseBusiness>.Ok(entity.ToBusiness(), "Course created");
    }

    public async Task<BusinessResult<CourseBusiness>> UpdateAsync(int id, CourseBusiness input)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null) return BusinessResult<CourseBusiness>.NotFound($"Course {id} not found");
        if (!await _semesterRepo.ExistsAsync(input.SemesterId))
            return BusinessResult<CourseBusiness>.Fail($"Semester {input.SemesterId} does not exist");
        input.ApplyTo(entity);
        await _repo.UpdateAsync(entity);
        return BusinessResult<CourseBusiness>.Ok(entity.ToBusiness(), "Course updated");
    }

    public async Task<BusinessResult<bool>> DeleteAsync(int id)
    {
        var ok = await _repo.DeleteAsync(id);
        return ok
            ? BusinessResult<bool>.Ok(true, "Course deleted")
            : BusinessResult<bool>.NotFound($"Course {id} not found");
    }

    public async Task<BusinessResult<bool>> ExistsAsync(int id)
        => BusinessResult<bool>.Ok(await _repo.ExistsAsync(id));

    public async Task<BusinessResult<PagedResult<EnrollmentBusiness>>> ListEnrollmentsAsync(int courseId, QueryOptions opts)
    {
        if (!await _repo.ExistsAsync(courseId))
            return BusinessResult<PagedResult<EnrollmentBusiness>>.NotFound($"Course {courseId} not found");
        try
        {
            var q = _repo.QueryEnrollmentsByCourseId(courseId);
            q = QueryHelper.ApplySearch(q, opts.Search, e => e.Status);
            q = QueryHelper.ApplySort(q, opts.Sort,
                new(StringComparer.OrdinalIgnoreCase) { "enrollmentId", "studentId", "courseId", "enrollDate", "status" },
                defaultSort: "-enrollDate");
            var expandStudent = QueryHelper.ShouldExpand(opts.Expand, "student");
            var expandCourse = QueryHelper.ShouldExpand(opts.Expand, "course");
            if (expandStudent) q = q.Include(e => e.Student);
            if (expandCourse) q = q.Include(e => e.Course).ThenInclude(c => c.Semester);
            // Always include Student + Course for flattened response (already part of IQueryable)
            var paged = await QueryHelper.ToPagedAsync(q, opts.Page, opts.Size,
                e => e.ToBusiness(expandStudent, expandCourse));
            return BusinessResult<PagedResult<EnrollmentBusiness>>.Ok(paged);
        }
        catch (ArgumentException ex) { return BusinessResult<PagedResult<EnrollmentBusiness>>.Fail(ex.Message); }
    }
}
