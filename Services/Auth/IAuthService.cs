using TicketSystemApi.Common;
using TicketSystemApi.Dtos;

namespace TicketSystemApi.Services.Auth;

public interface IAuthService
{
    Task<LoginResultDto?> LoginAsync(string email, string password, string? ip, string? userAgent);
    Task<ServiceResult<UserDto>> RegisterAsync(RegisterDto dto);
    Task<RefreshResultDto> RefreshAsync(string refreshTokenPlain, string? ip, string? userAgent);
    Task<bool> LogoutAsync(string refreshTokenPlain, string? ip);
}