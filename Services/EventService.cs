using AutoMapper;
using TicketSystemApi.Common;
using TicketSystemApi.Dtos;
using TicketSystemApi.Models;
using TicketSystemApi.Repositories;
using TicketSystemApi.Services;

namespace TicketSystemApi.Services;

public class EventService(IEventRepository eventRepository, IOrderRepository orderRepository, IMapper mapper) : IEventService
{
    private readonly IEventRepository _eventRepository = eventRepository;

    private readonly IOrderRepository _orderRepository = orderRepository;

    private readonly IMapper _mapper = mapper;

    public async Task<ServiceResult<IEnumerable<EventDto>>> GetAllAsync()
    {
        var events = await _eventRepository.GetAllAsync();

        if (events == null)
        {
            return ServiceResult<IEnumerable<EventDto>>.Fail("目前無活動場次");
        }

        var respDto = _mapper.Map<IEnumerable<EventDto>>(events);
        return ServiceResult<IEnumerable<EventDto>>.Ok(respDto);
    }

    public async Task<ServiceResult<EventDto>> GetByIdAsync(int id)
    {
        var events = await _eventRepository.GetByIdAsync(id);

        if (events == null)
        {
            return ServiceResult<EventDto>.Fail("查無此活動場次");
        }

        var respDto = _mapper.Map<EventDto>(events);
        return ServiceResult<EventDto>.Ok(respDto);
    }

    public async Task<ServiceResult<EventDto>> CreateAsync(EventCreateDto evt)
    {
        var events = _mapper.Map<Event>(evt);

        await _eventRepository.AddAsync(events);

        var respDto = _mapper.Map<EventDto>(events);

        return ServiceResult<EventDto>.Ok(respDto);
    }

    public async Task<ServiceResult> UpdateAsync(int id, EventUpdateDto evt)
    {
        if (id != evt.Id)
        {
            return ServiceResult.Fail("路由ID與資料ID不相同");
        }

        if (!await _eventRepository.ExistsAsync(id))
        {
            return ServiceResult.Fail("活動場次不存在，無法更新");
        }

        var events = _mapper.Map<Event>(evt);

        await _eventRepository.UpdateAsync(events);

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var evt = await _eventRepository.GetByIdAsync(id);

        if (evt == null)
        {
            return ServiceResult.Fail("活動場次不存在，無法刪除");
        }

        if (await _orderRepository.HasOrderForEventAsync(id))
        {
            return ServiceResult.Fail("無法刪除，因為有訂單與此活動場次相關聯");
        }

        await _eventRepository.DeleteAsync(evt);

        return ServiceResult.Ok();
    }
}