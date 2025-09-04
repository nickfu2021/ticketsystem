namespace TicketSystemApi.Services.Auth;

public interface ITokenService
{
    string CreateToken(string userId, IEnumerable<string>? roles = null);
}
