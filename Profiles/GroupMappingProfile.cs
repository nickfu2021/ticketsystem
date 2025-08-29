using TicketSystemApi.Dtos;
using TicketSystemApi.Models;

namespace TicketSystemApi.Profiles;

public class GroupMappingProfile : AutoMapper.Profile
{
    public GroupMappingProfile()
    {
        CreateMap<Group, GroupDto>();
        CreateMap<GroupCreateDto, Group>()
            .ForMember(dest => dest.CreatedAt,
                        opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt,
                        opt => opt.MapFrom(_ => DateTime.UtcNow));
        CreateMap<GroupUpdateDto, Group>()
            .ForMember(dest => dest.UpdatedAt,
                        opt => opt.MapFrom(_ => DateTime.UtcNow));            
    }
}