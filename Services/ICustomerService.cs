using TicketSystemApi.Common;
using TicketSystemApi.Models;

namespace TicketSystemApi.Services;

public interface ICustomerService
{
    Task<ServiceResult<IEnumerable<Customer>>> GetAllAsync();
    Task<ServiceResult<Customer>> GetByIdAsync(int id);
    Task<ServiceResult<Customer>> CreateAsync(Customer customer);
    Task<ServiceResult> UpdateAsync(int id, Customer customer);
    Task<ServiceResult> DeleteAsync(int id);
}