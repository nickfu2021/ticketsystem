using TicketSystemApi.Models;

namespace TicketSystemApi.Repositories;

public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId);
    Task<Order?> GetByIdAsync(int id);
    Task<Order> CreateAsync(Order order);
    Task UpdateAsync(Order order);
    Task DeleteAsync(Order order);
    Task<bool> ExistsAsync(int id);
    Task<int> GetSoldCountAsync(int eventId);
}