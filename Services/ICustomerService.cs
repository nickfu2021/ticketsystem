using TicketSystemApi.Common;
using TicketSystemApi.Dtos;
using TicketSystemApi.Models;

namespace TicketSystemApi.Services;

public interface ICustomerService
{
    Task<ServiceResult<IEnumerable<CustomerDto>>> GetAllAsync();
    Task<ServiceResult<CustomerDto>> GetByIdAsync(int id);
    Task<ServiceResult<CustomerDto>> CreateAsync(CustomerCreateDto customer);
    Task<ServiceResult<Unit>> UpdateAsync(int id, CustomerUpdateDto customer);
    Task<ServiceResult<Unit>> DeleteAsync(int id);
}