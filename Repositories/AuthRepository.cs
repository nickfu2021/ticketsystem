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
    public async Task CreateUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }
    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }
    public async Task<bool> IdNumberExistsAsync(string idNumber)
    {
        return await _context.Users.AnyAsync(u => u.IdNumber == idNumber);
    }

    // === Refresh Token 部分 ===

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
}