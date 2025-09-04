using System.Text;
using AutoMapper;
using Microsoft.Extensions.Options;
using TicketSystemApi.Common;
using TicketSystemApi.Configurations;
using TicketSystemApi.Dtos;
using TicketSystemApi.Models;
using TicketSystemApi.Repositories;

namespace TicketSystemApi.Services.Auth;

public class AuthService(IAuthRepository authRepository, IPostalRepository postalRepository, ITokenService tokenService, IEmailService emailService, IMapper mapper, ILogger<AuthService> logger, IOptions<JwtSettings> jwtOptions, IOptions<FrontendSettings> frontend) : IAuthService
{
    private readonly IAuthRepository _authRepository = authRepository;
    private readonly IPostalRepository _postalRepository = postalRepository;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IEmailService _emailService = emailService;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger _logger = logger;
    private readonly JwtSettings _jwt = jwtOptions.Value;
    private readonly FrontendSettings _frontend = frontend.Value;
    public async Task<(ServiceResult<LoginResultDto> Result, string? RefreshToken)> LoginAsync(string email, string password, string ip, string userAgent)
    {
        var user = await _authRepository.GetUserByEmailAsync(email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return (ServiceResult<LoginResultDto>.Fail("帳號或密碼錯誤"), null);

        // 1) Access Token（短效）: 與 respone json data 回傳
        var accessToken = _tokenService.CreateToken(user.UserUuid.ToString());

        // 2) Refresh Token（長效，不是JWT）: 只回傳到前端 cookie
        var rtPlain = RefreshTokenUtil.GeneratePlainToken();
        // 開發階段會讀 /Properties/launchSettings.json 注入
        var rtHash = RefreshTokenUtil.Hash(rtPlain, _jwt.RefreshTokenPepper);

        var rt = new RefreshToken
        {
            UserUuid = user.UserUuid,
            TokenHash = rtHash,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ip,
            UserAgent = userAgent,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays)
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
    public async Task<ServiceResult<Unit>> RegisterAsync(RegisterDto dto, string? ip, string? ua)
    {
        // 1) 基本資料驗證（商業規則）
        if (!string.IsNullOrWhiteSpace(dto.PostalCode) && !string.IsNullOrWhiteSpace(dto.City) && !string.IsNullOrWhiteSpace(dto.District))
        {
            bool postalValid = await _postalRepository.ExistsZipCityDistrictAsync(dto.PostalCode, dto.City, dto.District);
            if (!postalValid)
                return ServiceResult<Unit>.Fail("郵遞區號與縣市/鄉鎮區不符");
        }

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            if (await _authRepository.EmailExistsAsync(dto.Email))
                return ServiceResult<Unit>.Fail("此信箱已被註冊");
        }

        if (!string.IsNullOrWhiteSpace(dto.IdNumber))
        {
            if (await _authRepository.IdNumberExistsAsync(dto.IdNumber))
                return ServiceResult<Unit>.Fail("此身分證號已註冊");
        }

        if (!string.IsNullOrWhiteSpace(dto.MobileNumber))
        {
            if (await _authRepository.MobileExistsAsync(dto.MobileNumber))
                return ServiceResult<Unit>.Fail("此手機號碼已被使用");
        }

        var newUser = _mapper.Map<User>(dto);
        newUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        // 2) 建立使用者（local 註冊預設未啟用）
        var now = DateTime.UtcNow;
        var user = new User
        {
            UserUuid = Guid.NewGuid(),
            Email = dto.Email.Trim().ToLowerInvariant(),
            AuthProvider = "local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Username = string.IsNullOrWhiteSpace(dto.Username) ? null : dto.Username,
            IdNumber = string.IsNullOrWhiteSpace(dto.IdNumber) ? null : dto.IdNumber,
            Birthday = string.IsNullOrWhiteSpace(dto.Birthday) ? null : dto.Birthday,
            MobileNumber = string.IsNullOrWhiteSpace(dto.MobileNumber) ? null : dto.MobileNumber,
            PostalCode = string.IsNullOrWhiteSpace(dto.PostalCode) ? null : dto.PostalCode,
            Address = string.IsNullOrWhiteSpace(dto.AddressDetail) ? null : dto.AddressDetail,
            IsActive = false,          // email 未驗證前不啟用
            IsLocked = false,
            CreatedAt = now,
            UpdatedAt = now,
            EmailVerifiedAt = null,
            LastLoginAt = null
        };

        // 3) 建立 Email 驗證 Token（只把 hash 存 DB；純文字放信中）
        var plainToken = RefreshTokenUtil.GeneratePlainToken(64); // 高熵字串
        var tokenHash = RefreshTokenUtil.Hash(plainToken, _jwt.RefreshTokenPepper);

        var evt = new EmailVerificationToken
        {
            TokenId = Guid.NewGuid(),
            UserUuid = user.UserUuid,
            TokenHash = tokenHash,
            ExpiresAt = now.AddHours(24),
            CreatedAt = now,
            CreatedIp = ip,
            CreatedUa = ua
        };

        // 4) 儲存（建議同一個交易）
        await _authRepository.CreateUserAsync(user);
        await _authRepository.AddEmailVerificationTokenAsync(evt);
        await _authRepository.SaveChangesAsync();

        // 5) 寄出驗證信
        var link = $"{_frontend.BaseUrl}/auth/verify-email?uid={user.UserUuid}&token={Uri.EscapeDataString(plainToken)}";
        var subject = "請完成您的 Email 驗證";
        var htmlBody = $@"
        <p>親愛的 {user.Username ?? "使用者"} 您好，</p>
        <p>請點擊以下連結完成註冊：</p>
        <p><a href='{link}'>{link}</a></p>
        <p>若您未申請註冊，請忽略此封信。</p>
        <p>此連結 24 小時內有效。</p>";

        // 5) 寄送信件
        await _emailService.SendAsync(user.Email, subject, htmlBody);

        // 6) 統一回傳格式（不回傳個資）
        return ServiceResult<Unit>.Ok(Unit.Value);
    }

    public async Task<(RefreshResultDto Result, string? NewRtPlain)> RefreshAsync(string refreshTokenPlain, string? ip = null, string? userAgent = null)
    {
        var now = DateTime.UtcNow;

        var hash = RefreshTokenUtil.Hash(refreshTokenPlain, _jwt.RefreshTokenPepper);
        var dbToken = await _authRepository.GetActiveRefreshTokenByHashAsync(hash);
        // 1. 找不到或被撤銷的 Token
        if (dbToken == null)
        {
            return (new RefreshResultDto { Ok = false, Error = "invalid_refresh_token" }, null);
        }

        // 2. 被撤銷 + 被替換（表示是「舊 RT」被重複使用，屬資安攻擊）
        if (!dbToken.IsActive && dbToken.ReplacedBy != null)
        {
            // 封鎖該帳號
            var lockedUser = await _authRepository.GetUserByUuidAsync(dbToken.UserUuid);
            if (lockedUser != null && !lockedUser.IsLocked)
            {
                lockedUser.IsLocked = true;
                await _authRepository.SaveChangesAsync();

                // 寄送警告 Email（自行實作此方法）
                await _emailService.SendSecurityAlertAsync(lockedUser.Email, ip, userAgent);

                // 可加上 logger 記錄
                _logger?.LogWarning("使用者 {Email} 因重複使用舊 RefreshToken 而被封鎖", lockedUser.Email);
            }

            return (new RefreshResultDto { Ok = false, Error = "reused_refresh_token_blocked" }, null);
        }

        if (!dbToken.IsActive || dbToken.ExpiresAt <= now)
            return (new RefreshResultDto { Ok = false, Error = "refresh_token_inactive" }, null);

        var user = await _authRepository.GetUserByUuidAsync(dbToken.UserUuid);
        if (user is null)
            return (new RefreshResultDto { Ok = false, Error = "user_not_found" }, null);

        if (user.IsLocked)
            return (new RefreshResultDto { Ok = false, Error = "account_locked" }, null);

        if (!user.IsActive)
            return (new RefreshResultDto { Ok = false, Error = "email_not_verified" }, null);


        dbToken.LastUsedAt = now;
        dbToken.LastUsedIp = ip;

        var accessToken = _tokenService.CreateToken(dbToken.UserUuid.ToString());

        // 產生新的 RT（旋轉）
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

        // 撤銷舊 RT，並記錄被誰取代
        await _authRepository.RevokeRefreshTokenAsync(dbToken, reason: "rotation", newHash);
        await _authRepository.AddRefreshTokenAsync(newRt);
        await _authRepository.SaveChangesAsync();

        var result = new RefreshResultDto
        {
            Ok = true,
            AccessToken = accessToken,
            ExpiresIn = _jwt.ExpiresMinutes * 60 // 可選
        };
        return (result, newRtPlain);
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

    public async Task<ServiceResult<Unit>> VerifyEmailAsync(Guid userUuid, string tokenPlain)
    {
        // 1) 找使用者
        var user = await _authRepository.GetUserByUuidAsync(userUuid);
        if (user is null)
            return ServiceResult<Unit>.Fail("user_not_found");

        // 已驗證就直接成功（避免暴露太多訊息）
        if (user.EmailVerifiedAt is not null)
            return ServiceResult<Unit>.Ok(Unit.Value);

        // 2) 找「尚未使用」且「未過期」的驗證 token（可取最新一筆）
        var token = await _authRepository.GetLatestActiveEmailTokenAsync(userUuid);
        if (token is null)
            return ServiceResult<Unit>.Fail("verification_token_not_found");

        // 3) 檢查是否過期
        var now = DateTime.UtcNow;
        if (token.ExpiresAt <= now)
            return ServiceResult<Unit>.Fail("verification_token_expired");

        // 4) 計算 hash 並做「固定時間比較」
        var computedHash = RefreshTokenUtil.Hash(tokenPlain, _jwt.RefreshTokenPepper);

        // 使用固定時間比較，避免時序攻擊
        var a = Encoding.UTF8.GetBytes(computedHash);
        var b = Encoding.UTF8.GetBytes(token.TokenHash);
        if (!System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(a, b))
            return ServiceResult<Unit>.Fail("verification_token_invalid");

        // 5) 標記 token 已使用 + 啟用帳號
        token.ConsumedAt = now;
        user.EmailVerifiedAt = now;
        user.IsActive = true;
        user.UpdatedAt = now;

        await _authRepository.SaveChangesAsync();

        return ServiceResult<Unit>.Ok(Unit.Value);
    }

    public async Task<ServiceResult<Unit>> ResendVerificationAsync(string email, string? ip, string? ua)
    {
        // 1) 找使用者
        var user = await _authRepository.GetUserByEmailAsync(email.Trim().ToLowerInvariant());
        if (user is null)
        {
            // 避免暴露帳號存在與否 → 回 OK
            return ServiceResult<Unit>.Ok(Unit.Value);
        }

        // 已驗證 → 直接 OK
        if (user.EmailVerifiedAt is not null)
        {
            return ServiceResult<Unit>.Ok(Unit.Value);
        }

        // 2) 撤銷該使用者所有未使用的驗證 token（避免混淆）
        await _authRepository.RevokeUnconsumedEmailTokensAsync(user.UserUuid);

        // 3) 產生新的驗證 token
        var plainToken = RefreshTokenUtil.GeneratePlainToken(64);
        var tokenHash = RefreshTokenUtil.Hash(plainToken, _jwt.RefreshTokenPepper);

        var evt = new EmailVerificationToken
        {
            TokenId = Guid.NewGuid(),
            UserUuid = user.UserUuid,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow,
            CreatedIp = ip,
            CreatedUa = ua
        };
        await _authRepository.AddEmailVerificationTokenAsync(evt);
        await _authRepository.SaveChangesAsync();

        // 4) 發送新驗證信
        var link = $"{_frontend.BaseUrl}/auth/verify-email?uid={user.UserUuid}&token={Uri.EscapeDataString(plainToken)}";
        await _emailService.SendEmailVerificationAsync(user.Email, user.Username ?? user.Email, link);

        return ServiceResult<Unit>.Ok(Unit.Value);
    }

}
