using AutoMapper;
using TicketSystemApi.Dtos;
using TicketSystemApi.Models;

namespace TicketSystemApi.Profiles;

public class EventMappingProfile : Profile
{
    public EventMappingProfile()
    {
        CreateMap<Event, EventDto>();
        CreateMap<EventCreateDto, Event>();
        CreateMap<EventUpdateDto, Event>();
    }
}