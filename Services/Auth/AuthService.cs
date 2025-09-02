using AutoMapper;
using Microsoft.Extensions.Options;
using TicketSystemApi.Common;
using TicketSystemApi.Dtos;
using TicketSystemApi.Models;
using TicketSystemApi.Repositories;
using TicketSystemApi.Services.Auth;

namespace TicketSystemApi.Services.Auth;

public class AuthService(IAuthRepository authRepository, IPostalRepository postalRepository, ITokenService tokenService, IMapper mapper, IOptions<JwtSettings> jwtOptions) : IAuthService
{
    private readonly IAuthRepository _authRepository = authRepository;
    private readonly IPostalRepository _postalRepository = postalRepository;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IMapper _mapper = mapper;
    private readonly JwtSettings _jwt = jwtOptions.Value;

    public async Task<(ServiceResult<LoginResultDto> Result, string? RefreshToken)> LoginAsync(string email, string password, string ip, string userAgent)
    {
        var user = await _authRepository.GetUserByEmailAsync(email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return (ServiceResult<LoginResultDto>.Fail("帳號或密碼錯誤"), null);

        // 1) Access Token（短效）: 與 respone json data 回傳
        var accessToken = _tokenService.CreateToken(user.UserUuid.ToString(), user.Email);

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
    public async Task<ServiceResult<UserDto>> RegisterAsync(RegisterDto dto)
    {
        bool postalValid = await _postalRepository.ExistsZipCityDistrictAsync(dto.PostalCode, dto.City, dto.District);
        if (!postalValid)
            return ServiceResult<UserDto>.Fail("郵遞區號與縣市/鄉鎮區不符");

        if (await _authRepository.IdNumberExistsAsync(dto.IdNumber))
            return ServiceResult<UserDto>.Fail("此身分證號已註冊");

        if (await _authRepository.EmailExistsAsync(dto.Email))
            return ServiceResult<UserDto>.Fail("此信箱已被註冊");

        var newUser = _mapper.Map<User>(dto);
        newUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        await _authRepository.CreateUserAsync(newUser);
        var userDto = _mapper.Map<UserDto>(newUser);
        return ServiceResult<UserDto>.Ok(userDto);
    }

    public async Task<RefreshResultDto> RefreshAsync(string refreshTokenPlain, string? ip = null, string? userAgent = null)
    {
        var hash = RefreshTokenUtil.Hash(refreshTokenPlain, _jwt.RefreshTokenPepper);
        var dbToken = await _authRepository.GetActiveRefreshTokenByHashAsync(hash);
        if (dbToken == null || !dbToken.IsActive)
        {
            return new RefreshResultDto { Ok = false, Error = "invalid_refresh_token" };
        }

        // 補紀錄使用資訊（可選）
        dbToken.LastUsedAt = DateTime.UtcNow;
        dbToken.LastUsedIp = ip;

        // 1) 簽新 AT
        // 取得使用者（用 user_uuid 查你自己的 User Repo）
        var user = await _authRepository.GetUserByUuidAsync(dbToken.UserUuid);


        var accessToken = _tokenService.CreateToken(dbToken.UserUuid.ToString(), user!.Email);

        // 2) 產生新的 RT（旋轉）
        var newRtPlain = RefreshTokenUtil.GeneratePlainToken();
        var newHash = RefreshTokenUtil.Hash(newRtPlain, _jwt.RefreshTokenPepper);

        var newRt = new RefreshToken
        {
            UserUuid = dbToken.UserUuid,
            TokenHash = newHash,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ip,
            UserAgent = userAgent,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays)
        };

        // 撤銷舊 RT，並記錄被誰取代
        await _authRepository.RevokeRefreshTokenAsync(dbToken, reason: "rotation", replacedByHash: newHash);
        await _authRepository.AddRefreshTokenAsync(newRt);
        await _authRepository.SaveChangesAsync();

        return new RefreshResultDto
        {
            Ok = true,
            AccessToken = accessToken,
            NewRefreshToken = newRtPlain
        };
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
