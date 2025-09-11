using System.Text;
using Microsoft.Extensions.Options;
using TicketSystemApi.Common;
using TicketSystemApi.Configurations;
using TicketSystemApi.Dtos;
using TicketSystemApi.Models;
using TicketSystemApi.Repositories;
using TicketSystemApi.Utils;

namespace TicketSystemApi.Services.Auth;

// 這支不需要 IMapper，移除依賴避免 DI 錯誤
public class AuthService(
    IAuthRepository authRepository,
    IPostalRepository postalRepository,
    ITokenService tokenService,
    IEmailService emailService,
    ILogger<AuthService> logger,
    IOptions<JwtSettings> jwtOptions,
    IConfiguration config
) : IAuthService
{
    private readonly IAuthRepository _authRepository = authRepository;
    private readonly IPostalRepository _postalRepository = postalRepository;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IEmailService _emailService = emailService;
    private readonly ILogger _logger = logger;
    private readonly JwtSettings _jwt = jwtOptions.Value;
    private readonly IConfiguration _config = config;

    // --- Login ---

    public async Task<(ServiceResult<LoginResultDto> Result, string? RefreshToken)> LoginAsync(string email, string password, string ip, string userAgent)
    {
        var user = await _authRepository.GetUserByEmailAsync(email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return (ServiceResult<LoginResultDto>.Fail("帳號或密碼錯誤"), null);

        var accessToken = _tokenService.CreateToken(user.UserUuid.ToString());

        // 產生 RT（純文字給 cookie；雜湊入庫）
        var rtPlain = RefreshTokenUtil.GeneratePlainToken();
        var rtHash = RefreshTokenUtil.Hash(rtPlain, _jwt.RefreshTokenPepper);

        var rt = new RefreshToken
        {
            UserUuid = user.UserUuid,
            TokenHash = rtHash,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ip,
            UserAgent = userAgent,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays),
            IsActive = true
        };
        await _authRepository.AddRefreshTokenAsync(rt);
        await _authRepository.SaveChangesAsync();

        var dto = new LoginResultDto
        {
            AccessToken = accessToken,
            ExpiresIn = _jwt.ExpiresMinutes * 60,
            UserName = user.Username
        };

        return (ServiceResult<LoginResultDto>.Ok(dto), rtPlain);
    }

    // --- Register ---

    public async Task<ServiceResult<Unit>> RegisterAsync(RegisterDto dto, string? ip, string? ua)
    {
        if (!string.IsNullOrWhiteSpace(dto.PostalCode) && string.IsNullOrWhiteSpace(dto.City) && string.IsNullOrWhiteSpace(dto.District))
            return ServiceResult<Unit>.Fail("請填寫地址");

        // 基本商規檢查
        if (!string.IsNullOrWhiteSpace(dto.PostalCode) &&
            !string.IsNullOrWhiteSpace(dto.City) &&
            !string.IsNullOrWhiteSpace(dto.District))
        {
            var postalValid = await _postalRepository.ExistsZipCityDistrictAsync(
                dto.PostalCode, dto.City, dto.District);

            if (!postalValid) return ServiceResult<Unit>.Fail("郵遞區號與縣市/鄉鎮區不符");
        }

        if (!string.IsNullOrWhiteSpace(dto.Email) &&
            await _authRepository.EmailExistsAsync(dto.Email))
            return ServiceResult<Unit>.Fail("此信箱已被註冊");

        if (!string.IsNullOrWhiteSpace(dto.IdNumber) &&
            await _authRepository.IdNumberExistsAsync(dto.IdNumber))
            return ServiceResult<Unit>.Fail("此身分證號已註冊");

        if (!string.IsNullOrWhiteSpace(dto.MobileNumber) &&
            await _authRepository.MobileExistsAsync(dto.MobileNumber))
            return ServiceResult<Unit>.Fail("此手機號碼已被使用");

        // 建立使用者（未啟用，等驗證）
        var now = DateTime.UtcNow;
        var user = new User
        {
            UserUuid = Guid.NewGuid(),
            Email = dto.Email.Trim().ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Username = string.IsNullOrWhiteSpace(dto.Username) ? null : dto.Username,
            IdNumber = string.IsNullOrWhiteSpace(dto.IdNumber) ? null : dto.IdNumber,
            Birthday = string.IsNullOrWhiteSpace(dto.Birthday) ? null : dto.Birthday,
            MobileNumber = dto.MobileNumber,
            PostalCode = string.IsNullOrWhiteSpace(dto.PostalCode) ? null : dto.PostalCode,
            Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address,
            IsActive = false,
            IsLocked = false,
            CreatedAt = now,
            UpdatedAt = now,
            EmailVerifiedAt = null,
            LastLoginAt = null
        };

        // 產生 email 驗證 token（目前你先不寄信，保留邏輯）
        var plainToken = SecureTokenUtil.GenerateToken(64);
        var tokenHash = SecureTokenUtil.Hash(plainToken, _jwt.RefreshTokenPepper);
        var userToken = new UserToken
        {
            TokenUuid = Guid.NewGuid(),
            UserUuid = user.UserUuid,
            Purpose = "email_verify",
            TokenHash = tokenHash,
            CreatedAt = now,
            ExpiresAt = now.AddHours(24),
            ConsumedAt = null,
            RevokedAt = null,
            IpCreated = ip,
            UaCreated = ua,
            Meta = "{}"
        };

        await _authRepository.CreateUserAsync(user);
        await _authRepository.AddEmailVerificationTokenAsync(userToken);

        // 寄信（你目前公司網路擋 SMTP，先保留）

        var link = $"{_config["Email:VerifyBaseUrl"]}?token={Uri.EscapeDataString(plainToken)}";
        await _emailService.SendAsync(user.Email, "請完成您的 Email 驗證", $"<a href='{link}'>{link}</a>");

        return ServiceResult<Unit>.Ok(Unit.Value);
    }

    // --- Refresh（重點：改成回 ServiceResult<TokenDto>，不再有 Ok/Error 欄位） ---

    public async Task<(ServiceResult<TokenDto> Result, string? NewRtPlain)> RefreshAsync(
        string refreshTokenPlain, string? ip = null, string? userAgent = null)
    {
        var now = DateTime.UtcNow;
        var hash = RefreshTokenUtil.Hash(refreshTokenPlain, _jwt.RefreshTokenPepper);
        var dbToken = await _authRepository.GetActiveRefreshTokenByHashAsync(hash);

        if (dbToken == null)
            return (ServiceResult<TokenDto>.Fail("invalid_refresh_token"), null);

        if (!dbToken.IsActive && dbToken.ReplacedBy != null)
        {
            var lockedUser = await _authRepository.GetUserByUuidAsync(dbToken.UserUuid);
            if (lockedUser != null && !lockedUser.IsLocked)
            {
                lockedUser.IsLocked = true;
                await _authRepository.SaveChangesAsync();
                await _emailService.SendSecurityAlertAsync(lockedUser.Email, ip, userAgent);
                _logger?.LogWarning("使用者 {Email} 因重複使用舊 RefreshToken 而被封鎖", lockedUser.Email);
            }
            return (ServiceResult<TokenDto>.Fail("reused_refresh_token_blocked"), null);
        }

        if (!dbToken.IsActive || dbToken.ExpiresAt <= now)
            return (ServiceResult<TokenDto>.Fail("refresh_token_inactive"), null);

        var user = await _authRepository.GetUserByUuidAsync(dbToken.UserUuid);
        if (user is null)
            return (ServiceResult<TokenDto>.Fail("user_not_found"), null);

        if (user.IsLocked)
            return (ServiceResult<TokenDto>.Fail("account_locked"), null);

        if (!user.IsActive)
            return (ServiceResult<TokenDto>.Fail("email_not_verified"), null);

        dbToken.LastUsedAt = now;
        dbToken.LastUsedIp = ip;

        var accessToken = _tokenService.CreateToken(dbToken.UserUuid.ToString());

        // 旋轉新的 RT
        var newRtPlain = RefreshTokenUtil.GeneratePlainToken();
        var newHash = RefreshTokenUtil.Hash(newRtPlain, _jwt.RefreshTokenPepper);

        var newRt = new RefreshToken
        {
            UserUuid = dbToken.UserUuid,
            TokenHash = newHash,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ip,
            UserAgent = userAgent,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays),
            IsActive = true
        };

        await _authRepository.RevokeRefreshTokenAsync(dbToken, reason: "rotation", replacedByHash: newHash);
        await _authRepository.AddRefreshTokenAsync(newRt);
        await _authRepository.SaveChangesAsync();

        var dto = new TokenDto
        {
            AccessToken = accessToken,
            ExpiresIn = _jwt.ExpiresMinutes * 60
        };
        return (ServiceResult<TokenDto>.Ok(dto), newRtPlain);
    }

    public async Task<bool> LogoutAsync(string refreshTokenPlain, string? ip = null)
    {
        var hash = RefreshTokenUtil.Hash(refreshTokenPlain, _jwt.RefreshTokenPepper);
        var dbToken = await _authRepository.GetActiveRefreshTokenByHashAsync(hash);
        if (dbToken == null) return false;

        await _authRepository.RevokeRefreshTokenAsync(dbToken, "logout");
        await _authRepository.SaveChangesAsync();
        return true;
    }

    public async Task<ServiceResult<Unit>> VerifyAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return ServiceResult<Unit>.Fail("missing_token");

        var hash = SecureTokenUtil.Hash(token, _jwt.RefreshTokenPepper);

        var dbToken = await _authRepository.GetActiveEmailVerifyTokenByHashAsync(hash);
        if (dbToken == null)
            return ServiceResult<Unit>.Fail("invalid_or_expired_token");

        var now = DateTime.UtcNow;

        await _authRepository.VerifyEmailAndConsumeTokenAsync(dbToken, now);

        return ServiceResult<Unit>.Ok(Unit.Value);
    }
}
