using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.Services.Interfaces;

public interface ISemesterService
{
    Task<BusinessResult<PagedResult<SemesterBusiness>>> ListAsync(QueryOptions opts);
    Task<BusinessResult<SemesterBusiness>> GetByIdAsync(int id, string? expand = null);
    Task<BusinessResult<SemesterBusiness>> CreateAsync(SemesterBusiness input);
    Task<BusinessResult<SemesterBusiness>> UpdateAsync(int id, SemesterBusiness input);
    Task<BusinessResult<bool>> DeleteAsync(int id);
    Task<BusinessResult<bool>> ExistsAsync(int id);
    Task<BusinessResult<PagedResult<CourseBusiness>>> ListCoursesAsync(int semesterId, QueryOptions opts);
}
