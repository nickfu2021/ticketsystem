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
    IOptions<EmailSettings> emailOptions
) : IAuthService
{
    private readonly IAuthRepository _authRepository = authRepository;
    private readonly IPostalRepository _postalRepository = postalRepository;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IEmailService _emailService = emailService;
    private readonly ILogger _logger = logger;
    private readonly JwtSettings _jwt = jwtOptions.Value;
    private readonly EmailSettings _email = emailOptions.Value;

    // 註冊
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
            ExpiresAt = now.AddHours(2),
            ConsumedAt = null,
            RevokedAt = null,
            IpCreated = ip,
            UaCreated = ua,
            Meta = "{}"
        };

        await _authRepository.CreateUserAsync(user);
        await _authRepository.AddEmailVerificationTokenAsync(userToken);

        // 寄信（你目前公司網路擋 SMTP，先保留）

        var link = $"{_email.VerifyBaseUrl}?token={Uri.EscapeDataString(plainToken)}";
        await _emailService.SendAsync(user.Email, "請完成您的 Email 驗證", $"<a href='{link}'>{link}</a>");

        return ServiceResult<Unit>.Ok(Unit.Value);
    }

    // 驗證信
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
    
    // 登入
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

    // 刷新 token
    public async Task<(ServiceResult<TokenDto> Result, string? NewRtPlain)> RefreshAsync(string refreshTokenPlain, string? ip = null, string? userAgent = null)
    {
        /*  RefreshAsync 程式功能概述
        
            驗證傳入的 Refresh Token 是否合法、是否過期、是否被重複使用。若合法，會：
            1.建立新的 Access Token（JWT）
            2.建立新的 Refresh Token
            3.舊的 Refresh Token 標記為「已取代」
        */
        var now = DateTime.UtcNow;

        // 把明文 rt 雜湊
        var hash = RefreshTokenUtil.Hash(refreshTokenPlain, _jwt.RefreshTokenPepper);

        // 到資料庫查是否存在這一筆 rt Hash
        var dbToken = await _authRepository.GetActiveRefreshTokenByHashAsync(hash);

        // 不合法、不存在的 refresh_token
        if (dbToken == null)
            return (ServiceResult<TokenDto>.Fail("invalid_refresh_token"), null);

        // 使用者若用「舊的」 Refresh Token（已被取代）來換新，系統會立即封鎖帳號並寄安全警示信。
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

        // 檢查是否過期、或標記為不活躍
        if (!dbToken.IsActive || dbToken.ExpiresAt <= now)
            return (ServiceResult<TokenDto>.Fail("refresh_token_inactive"), null);

        // 確認使用者狀態正常（未鎖、已啟用）
        var user = await _authRepository.GetUserByUuidAsync(dbToken.UserUuid);
        if (user is null)
            return (ServiceResult<TokenDto>.Fail("user_not_found"), null);

        if (user.IsLocked)
            return (ServiceResult<TokenDto>.Fail("account_locked"), null);

        if (!user.IsActive)
            return (ServiceResult<TokenDto>.Fail("email_not_verified"), null);

        dbToken.LastUsedAt = now;
        dbToken.LastUsedIp = ip;

        // 產生新的 AT
        var accessToken = _tokenService.CreateToken(dbToken.UserUuid.ToString());

        // 產生新的 RT
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

        // 把舊的 rt 標記 IsActive = false 壓上註銷
        await _authRepository.RevokeRefreshTokenAsync(dbToken, reason: "rotation", replacedByHash: newHash);

        // 建立新的 rt
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


}
