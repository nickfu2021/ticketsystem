using TicketSystemApi.Common;
using TicketSystemApi.Dtos;
using TicketSystemApi.Models;

namespace TicketSystemApi.Services;

public interface IOrderService
{
    Task<ServiceResult<OrderDto>> GetByIdAsync(int id);
    Task<ServiceResult<IEnumerable<OrderDto>>> GetByCustomerIdAsync(int customerId);
    Task<ServiceResult<OrderDto>> CreateAsync(OrderCreateDto order);
    Task<ServiceResult<Unit>> UpdateAsync(int id, OrderUpdateDto order);
    Task<ServiceResult<Unit>> DeleteAsync(int id);
}