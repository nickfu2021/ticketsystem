using TicketSystemApi.Models;
using TicketSystemApi.Repositories;
using TicketSystemApi.Services;

namespace TicketSystemApi.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _repository;

    public EventService(IEventRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Events>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Events?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Events> CreateAsync(Events evt)
    {
        await _repository.AddAsync(evt);
        return evt;
    }

    public async Task<bool> UpdateAsync(int id, Events evt)
    {
        if (!await _repository.ExistsAsync(id))
        {
            return false;
        }

        await _repository.UpdateAsync(evt);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var evt = await _repository.GetByIdAsync(id);
        if (evt == null)
        {
                return false;
        }

        await _repository.DeleteAsync(evt);
        return true;
    }
}