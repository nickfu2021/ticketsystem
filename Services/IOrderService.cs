using TicketSystemApi.Common;
using TicketSystemApi.Models;

namespace TicketSystemApi.Services;

public interface IOrderService
{
    Task<ServiceResult<IEnumerable<Order>>> GetByCustomerIdAsync(int customerId);
    Task<ServiceResult<Order>> CreateAsync(Order order);
    Task<ServiceResult> UpdateAsync(int id, Order order);
    Task<ServiceResult> DeleteAsync(int id);
}