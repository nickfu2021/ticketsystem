using BandHub.AuthService.Common;
using BandHub.AuthService.Dtos;

namespace BandHub.AuthService.Services.Auth;

public interface IAuthService
{
    Task<(ServiceResult<LoginResultDto> Result, string? RefreshToken)> LoginAsync(string email, string password, string ip, string userAgent);
    Task<ServiceResult<Unit>> RegisterAsync(RegisterDto dto, string? ip, string? ua);
    Task<(ServiceResult<TokenDto> Result, string? NewRtPlain)> RefreshAsync(string refreshTokenPlain, string? ip, string? userAgent);
    Task<bool> LogoutAsync(string refreshTokenPlain, string? ip);
    Task<ServiceResult<Unit>> VerifyAsync(string token);
}
