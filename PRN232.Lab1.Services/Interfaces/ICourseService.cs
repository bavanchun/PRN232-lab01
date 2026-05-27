using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.Services.Interfaces;

public interface ICourseService
{
    Task<BusinessResult<PagedResult<CourseBusiness>>> ListAsync(QueryOptions opts);
    Task<BusinessResult<CourseBusiness>> GetByIdAsync(int id, string? expand = null);
    Task<BusinessResult<CourseBusiness>> CreateAsync(CourseBusiness input);
    Task<BusinessResult<CourseBusiness>> UpdateAsync(int id, CourseBusiness input);
    Task<BusinessResult<bool>> DeleteAsync(int id);
    Task<BusinessResult<bool>> ExistsAsync(int id);
    Task<BusinessResult<PagedResult<EnrollmentBusiness>>> ListEnrollmentsAsync(int courseId, QueryOptions opts);
}
