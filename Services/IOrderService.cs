using TicketSystemApi.Common;
using TicketSystemApi.Dtos;
using TicketSystemApi.Models;

namespace TicketSystemApi.Services;

public interface IOrderService
{
    Task<ServiceResult<Order>> GetByIdAsync(int id);
    Task<ServiceResult<IEnumerable<Order>>> GetByCustomerIdAsync(int customerId);
    Task<ServiceResult<Order>> CreateAsync(OrderCreateDto order);
    Task<ServiceResult> UpdateAsync(int id, OrderUpdateDto order);
    Task<ServiceResult> DeleteAsync(int id);
}