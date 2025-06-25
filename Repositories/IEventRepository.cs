using TicketSystemApi.Models;

namespace TicketSystemApi.Repositories;

public interface IEventRepository
{
    Task<IEnumerable<Events>> GetAllAsync();
    Task<Events?> GetByIdAsync(int id);
    Task AddAsync(Events evt);
    Task UpdateAsync(Events evt);
    Task DeleteAsync(Events evt);
    Task<bool> ExistsAsync(int id);
}