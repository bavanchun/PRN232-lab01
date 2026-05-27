using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.Services.Interfaces;

public interface IEnrollmentService
{
    Task<BusinessResult<PagedResult<EnrollmentBusiness>>> ListAsync(QueryOptions opts);
    Task<BusinessResult<EnrollmentBusiness>> GetByIdAsync(int id, string? expand = null);
    Task<BusinessResult<EnrollmentBusiness>> CreateAsync(EnrollmentBusiness input);
    Task<BusinessResult<EnrollmentBusiness>> UpdateAsync(int id, EnrollmentBusiness input);
    Task<BusinessResult<bool>> DeleteAsync(int id);
    Task<BusinessResult<bool>> ExistsAsync(int id);
}
