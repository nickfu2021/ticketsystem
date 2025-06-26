using TicketSystemApi.Common;
using TicketSystemApi.Models;
using TicketSystemApi.Repositories;
using TicketSystemApi.Services;

namespace TicketSystemApi.Services;

public class EventService(IEventRepository repository) : IEventService
{
    private readonly IEventRepository _repository = repository;

    public async Task<ServiceResult<IEnumerable<Event>>> GetAllAsync()
    {
        var events = await _repository.GetAllAsync();
        if (events == null)
        {
            return ServiceResult<IEnumerable<Event>>.Fail("目前無活動場次");
        }
        return ServiceResult<IEnumerable<Event>>.Ok(events);
    }

    public async Task<ServiceResult<Event>> GetByIdAsync(int id)
    {
        var events = await _repository.GetByIdAsync(id);
        if (events == null)
        {
            return ServiceResult<Event>.Fail("查無此活動場次");
        }
        return ServiceResult<Event>.Ok(events);
    }

    public async Task<ServiceResult<Event>> CreateAsync(Event evt)
    {
        await _repository.AddAsync(evt);
        return ServiceResult<Event>.Ok(evt);
    }

    public async Task<ServiceResult> UpdateAsync(int id, Event evt)
    {
        if (!await _repository.ExistsAsync(id))
        {
            return ServiceResult.Fail("活動場次不存在，無法更新");
        }

        await _repository.UpdateAsync(evt);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var evt = await _repository.GetByIdAsync(id);
        if (evt == null)
        {
            return ServiceResult.Fail("活動場次不存在，無法刪除");
        }

        await _repository.DeleteAsync(evt);
        return ServiceResult.Ok();
    }
}