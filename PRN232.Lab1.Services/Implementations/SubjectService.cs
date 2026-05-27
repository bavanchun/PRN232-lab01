using PRN232.Lab1.Repositories.Interfaces;
using PRN232.Lab1.Services.Helpers;
using PRN232.Lab1.Services.Interfaces;
using PRN232.Lab1.Services.Mapping;
using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.Services.Implementations;

public class SubjectService : ISubjectService
{
    private readonly ISubjectRepository _repo;
    private static readonly HashSet<string> SortFields = new(StringComparer.OrdinalIgnoreCase)
        { "subjectId", "subjectCode", "subjectName", "credit" };

    public SubjectService(ISubjectRepository repo) { _repo = repo; }

    public async Task<BusinessResult<PagedResult<SubjectBusiness>>> ListAsync(QueryOptions opts)
    {
        try
        {
            var q = _repo.Query();
            q = QueryHelper.ApplySearch(q, opts.Search, s => s.SubjectCode, s => s.SubjectName);
            q = QueryHelper.ApplySort(q, opts.Sort, SortFields, defaultSort: "subjectId");
            var paged = await QueryHelper.ToPagedAsync(q, opts.Page, opts.Size, e => e.ToBusiness());
            return BusinessResult<PagedResult<SubjectBusiness>>.Ok(paged);
        }
        catch (ArgumentException ex) { return BusinessResult<PagedResult<SubjectBusiness>>.Fail(ex.Message); }
    }

    public async Task<BusinessResult<SubjectBusiness>> GetByIdAsync(int id, string? expand = null)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity is null
            ? BusinessResult<SubjectBusiness>.NotFound($"Subject {id} not found")
            : BusinessResult<SubjectBusiness>.Ok(entity.ToBusiness());
    }

    public async Task<BusinessResult<SubjectBusiness>> CreateAsync(SubjectBusiness input)
    {
        var entity = input.ToEntity();
        await _repo.AddAsync(entity);
        return BusinessResult<SubjectBusiness>.Ok(entity.ToBusiness(), "Subject created");
    }

    public async Task<BusinessResult<SubjectBusiness>> UpdateAsync(int id, SubjectBusiness input)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null) return BusinessResult<SubjectBusiness>.NotFound($"Subject {id} not found");
        input.ApplyTo(entity);
        await _repo.UpdateAsync(entity);
        return BusinessResult<SubjectBusiness>.Ok(entity.ToBusiness(), "Subject updated");
    }

    public async Task<BusinessResult<bool>> DeleteAsync(int id)
    {
        var ok = await _repo.DeleteAsync(id);
        return ok
            ? BusinessResult<bool>.Ok(true, "Subject deleted")
            : BusinessResult<bool>.NotFound($"Subject {id} not found");
    }

    public async Task<BusinessResult<bool>> ExistsAsync(int id)
        => BusinessResult<bool>.Ok(await _repo.ExistsAsync(id));
}
