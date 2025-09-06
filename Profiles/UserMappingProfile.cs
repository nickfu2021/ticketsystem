using TicketSystemApi.Dtos;
using TicketSystemApi.Models;

namespace TicketSystemApi.Profiles;

public class UserMappingProfile : AutoMapper.Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<UserCreateDto, User>();
        CreateMap<UserUpdateDto, User>();
        CreateMap<RegisterDto, User>()
            .ForMember(dest => dest.Address,
                       opt => opt.MapFrom(src => $"{src.City}{src.District}{src.Address}"))
            .ForMember(dest => dest.PasswordHash,
                       opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt,
                       opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt,
                       opt => opt.MapFrom(_ => DateTime.UtcNow));
    }
}