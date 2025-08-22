namespace TicketSystemApi.Services.Auth;

public interface ITokenService
{
    string CreateToken(string userId, string? email, IEnumerable<string>? roles = null);
}
