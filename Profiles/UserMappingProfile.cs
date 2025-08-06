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
    }
}