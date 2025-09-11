using TicketSystemApi.Models;

namespace TicketSystemApi.Repositories;

public interface IAuthRepository
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByUuidAsync(Guid userUuid);
    Task CreateUserAsync(User user);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> IdNumberExistsAsync(string idNumber);
    Task<bool> MobileExistsAsync(string mobileNumber);
    Task AddEmailVerificationTokenAsync(UserToken token);
    // Refresh Token 相關
    Task AddRefreshTokenAsync(RefreshToken token);
    Task<RefreshToken?> GetActiveRefreshTokenByHashAsync(string tokenHash);
    Task SaveChangesAsync();
    Task RevokeRefreshTokenAsync(RefreshToken token, string reason, string? replacedByHash = null);
    // Email 相關
    Task<UserToken?> GetActiveEmailVerifyTokenByHashAsync(byte[] tokenHash);
    Task VerifyEmailAndConsumeTokenAsync(UserToken token, DateTime now);
}
