using TicketSystemApi.Common;
using TicketSystemApi.Dtos;

namespace TicketSystemApi.Services.Auth;

public interface IAuthService
{
    Task<LoginResultDto?> LoginAsync(string email, string password);
    Task<ServiceResult<UserDto>> RegisterAsync(RegisterDto dto);
}   