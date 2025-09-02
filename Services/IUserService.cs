using TicketSystemApi.Common;
using TicketSystemApi.Dtos;

namespace TicketSystemApi.Services;

public interface IUserService
{
    Task<ServiceResult<IEnumerable<UserDto>>> GetAllAsync();
    Task<ServiceResult<UserDto>> GetByIdAsync(Guid guid);
    Task<ServiceResult<UserDto>> CreateAsync(UserCreateDto user);
    Task<ServiceResult<Unit>> UpdateAsync(Guid guid, UserUpdateDto user);
    Task<ServiceResult<Unit>> DeleteAsync(Guid guid);

}