using TicketSystemApi.Common;
using TicketSystemApi.Dtos;
using TicketSystemApi.Models;

namespace TicketSystemApi.Services;

public interface IEventService
{
    Task<ServiceResult<IEnumerable<EventDto>>> GetAllAsync();
    Task<ServiceResult<EventDto>> GetByIdAsync(int id);
    Task<ServiceResult<EventDto>> CreateAsync(EventCreateDto evt);
    Task<ServiceResult> UpdateAsync(int id, EventUpdateDto evt);
    Task<ServiceResult> DeleteAsync(int id);
}