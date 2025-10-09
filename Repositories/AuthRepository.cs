using Microsoft.EntityFrameworkCore;
using BandHub.AuthService.Data;
using BandHub.AuthService.Models;
using BandHub.AuthService.Repositories;

namespace BandHub.AuthService.Repositories;

public class AuthRepository(AppDbContext context) : IAuthRepository
{
    private readonly AppDbContext _context = context;

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => EF.Functions.ILike(u.Email, email));
    }
    public async Task<User?> GetUserByUuidAsync(Guid guid)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.UserUuid == guid);
    }

    // 建立使用者
    public Task CreateUserAsync(User user)
    {
        _context.Users.Add(user);
        return Task.CompletedTask;
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

    public Task AddRefreshTokenAsync(RefreshToken token)
    {
        _context.RefreshTokens.Add(token);
        return Task.CompletedTask;
    }

    public async Task<RefreshToken?> GetActiveRefreshTokenByHashAsync(string tokenHash)
    {
        return await _context.RefreshTokens
            .Where(t => t.TokenHash == tokenHash && t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync();
    }

    public Task RevokeRefreshTokenAsync(RefreshToken token, string reason, string? replacedByHash = null)
    {
        token.IsActive = false;
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
        // 撤銷舊的同類型未使用 token（情況:重複寄驗證信）
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

        // 補上新的一筆 token
        _context.UserTokens.Add(token);
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