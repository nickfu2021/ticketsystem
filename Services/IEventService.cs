using TicketSystemApi.Common;
using TicketSystemApi.Models;

namespace TicketSystemApi.Services;

public interface IEventService
{
    Task<ServiceResult<IEnumerable<Event>>> GetAllAsync();
    Task<ServiceResult<Event>> GetByIdAsync(int id);
    Task<ServiceResult<Event>> CreateAsync(Event evt);
    Task<ServiceResult> UpdateAsync(int id, Event evt);
    Task<ServiceResult> DeleteAsync(int id);
}