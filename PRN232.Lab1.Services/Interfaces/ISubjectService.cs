using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.Services.Interfaces;

public interface ISubjectService
{
    Task<BusinessResult<PagedResult<SubjectBusiness>>> ListAsync(QueryOptions opts);
    Task<BusinessResult<SubjectBusiness>> GetByIdAsync(int id, string? expand = null);
    Task<BusinessResult<SubjectBusiness>> CreateAsync(SubjectBusiness input);
    Task<BusinessResult<SubjectBusiness>> UpdateAsync(int id, SubjectBusiness input);
    Task<BusinessResult<bool>> DeleteAsync(int id);
    Task<BusinessResult<bool>> ExistsAsync(int id);
}
