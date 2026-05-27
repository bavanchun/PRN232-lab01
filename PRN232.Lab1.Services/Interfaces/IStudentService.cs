using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.Services.Interfaces;

public interface IStudentService
{
    Task<BusinessResult<PagedResult<StudentBusiness>>> ListAsync(QueryOptions opts);
    Task<BusinessResult<StudentBusiness>> GetByIdAsync(int id, string? expand = null);
    Task<BusinessResult<StudentBusiness>> CreateAsync(StudentBusiness input);
    Task<BusinessResult<StudentBusiness>> UpdateAsync(int id, StudentBusiness input);
    Task<BusinessResult<bool>> DeleteAsync(int id);
    Task<BusinessResult<bool>> ExistsAsync(int id);
    Task<BusinessResult<PagedResult<EnrollmentBusiness>>> ListEnrollmentsAsync(int studentId, QueryOptions opts);
}
