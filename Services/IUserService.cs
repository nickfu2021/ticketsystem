using TicketSystemApi.Common;
using TicketSystemApi.Dtos;

namespace TicketSystemApi.Services;

public interface IUserService
{
    Task<ServiceResult<IEnumerable<UserDto>>> GetAllAsync();
    Task<ServiceResult<UserDto>> GetByIdAsync(int id);
    Task<ServiceResult<UserDto>> CreateAsync(UserCreateDto user);
    Task<ServiceResult> UpdateAsync(int id, UserUpdateDto user);
    Task<ServiceResult> DeleteAsync(int id);

}