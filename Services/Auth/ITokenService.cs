namespace BandHub.AuthService.Services.Auth;

public interface ITokenService
{
    string CreateToken(string userId, IEnumerable<string>? roles = null);
}
