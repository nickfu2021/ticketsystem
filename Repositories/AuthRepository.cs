using Microsoft.EntityFrameworkCore;
using TicketSystemApi.Data;
using TicketSystemApi.Models;
using TicketSystemApi.Repositories;

namespace TicketSystemApi.Repositories;

public class AuthRepository(AppDbContext context) : IAuthRepository
{
    private readonly AppDbContext _context = context;

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
    public async Task<User?> GetUserByUuidAsync(Guid guid)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.UserUuid == guid);
    }

    // 建立使用者
    public async Task CreateUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }
    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => EF.Functions.ILike(u.Email, email));
    }
    public async Task<bool> IdNumberExistsAsync(string idNumber)
    {
        return await _context.Users.AnyAsync(u => u.IdNumber == idNumber);
    }
    public async Task<bool> MobileExistsAsync(string mobileNumber)
    {
        return await _context.Users.AnyAsync(u => u.MobileNumber == mobileNumber);
    }

    public async Task AddRefreshTokenAsync(RefreshToken token)
    {
        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync();
    }

    public async Task<RefreshToken?> GetActiveRefreshTokenByHashAsync(string tokenHash)
    {
        return await _context.RefreshTokens
            .Where(t => t.TokenHash == tokenHash && t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync();
    }

    public Task RevokeRefreshTokenAsync(RefreshToken token, string reason, string? replacedByHash = null)
    {
        token.RevokedAt = DateTime.UtcNow;
        token.RevokedReason = reason;
        token.ReplacedBy = replacedByHash;
        _context.RefreshTokens.Update(token);

        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    // 建立驗證 email token
    public async Task AddEmailVerificationTokenAsync(UserToken token)
    {
        // 可選：先撤銷舊的同類型未使用 token（確保唯一）
        var activeToken = await _context.UserTokens
            .Where(x => x.UserUuid == token.UserUuid &&
                        x.Purpose == "email_verify" &&
                        x.ConsumedAt == null &&
                        x.RevokedAt == null &&
                        x.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync();

        if (activeToken != null)
        {
            activeToken.RevokedAt = DateTime.UtcNow;
        }

        await _context.UserTokens.AddAsync(token);
        await _context.SaveChangesAsync();
    }

    // email token 是否存在
    public async Task<UserToken?> GetActiveEmailVerifyTokenByHashAsync(byte[] tokenHash)
    {
        return await _context.UserTokens
            .AsTracking()
            .FirstOrDefaultAsync(t =>
                t.Purpose == "email_verify" &&
                t.TokenHash == tokenHash &&
                t.RevokedAt == null &&
                t.ConsumedAt == null &&
                t.ExpiresAt > DateTime.UtcNow);
    }

    // email token 消耗
    public async Task VerifyEmailAndConsumeTokenAsync(UserToken token, DateTime now)
    {
        // 同個 DbContext 下原子提交
        var user = await _context.Users.FirstAsync(u => u.UserUuid == token.UserUuid);
        user.EmailVerifiedAt = now;
        user.IsActive = true;
        token.ConsumedAt = now;

        await _context.SaveChangesAsync();
    }
}