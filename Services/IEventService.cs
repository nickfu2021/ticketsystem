using TicketSystemApi.Models;

namespace TicketSystemApi.Services;

public interface IEventService
{
    Task<IEnumerable<Events>> GetAllAsync();
    Task<Events?> GetByIdAsync(int id);
    Task<Events> CreateAsync(Events evt);
    Task<bool> UpdateAsync(int id, Events evt);
    Task<bool> DeleteAsync(int id);
}