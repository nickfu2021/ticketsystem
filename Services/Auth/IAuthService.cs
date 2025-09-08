using TicketSystemApi.Common;
using TicketSystemApi.Dtos;

namespace TicketSystemApi.Services.Auth;

public interface IAuthService
{
    Task<(ServiceResult<LoginResultDto> Result, string? RefreshToken)> LoginAsync(string email, string password, string ip, string userAgent);
    Task<ServiceResult<Unit>> RegisterAsync(RegisterDto dto, string? ip, string? ua);
    Task<(ServiceResult<TokenDto> Result, string? NewRtPlain)> RefreshAsync(string refreshTokenPlain, string? ip, string? userAgent);
    Task<bool> LogoutAsync(string refreshTokenPlain, string? ip);
}
