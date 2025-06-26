using TicketSystemApi.Models;

namespace TicketSystemApi.Repositories;

public interface IEventRepository
{
    Task<IEnumerable<Event>> GetAllAsync();
    Task<Event?> GetByIdAsync(int id);
    Task AddAsync(Event evt);
    Task UpdateAsync(Event evt);
    Task DeleteAsync(Event evt);
    Task<bool> ExistsAsync(int id);
}