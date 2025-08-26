
using AutoMapper;
using TicketSystemApi.Common;
using TicketSystemApi.Dtos;
using TicketSystemApi.Models;
using TicketSystemApi.Repositories;
using TicketSystemApi.Services.Auth;

namespace TicketSystemApi.Services.Auth;

public class AuthService(IAuthRepository authRepository,IPostalRepository postalRepository, ITokenService tokenService, IMapper mapper) : IAuthService
{
    private readonly IAuthRepository _authRepository = authRepository;
    private readonly IPostalRepository _postalRepository = postalRepository;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IMapper _mapper = mapper;

    public async Task<LoginResultDto?> LoginAsync(string email, string password)
    {
        var user = await _authRepository.GetUserByEmailAsync(email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return null;
        }

        var token = _tokenService.CreateToken(user.Id.ToString(), user.Email);
        return new LoginResultDto
        {
            Token = token,
            UserName = user.Username
        };
    }
    public async Task<ServiceResult<UserDto>> RegisterAsync(RegisterDto dto)
    {
        bool postalValid = await _postalRepository.ExistsZipCityDistrictAsync(dto.PostalCode, dto.City, dto.District);
        if (!postalValid)
        {
            return ServiceResult<UserDto>.Fail("郵遞區號與縣市/鄉鎮區不符");
        }

        if (await _authRepository.IdNumberExistsAsync(dto.IdNumber))
        {
            return ServiceResult<UserDto>.Fail("此身分證號已註冊");
        }

        if ( await _authRepository.EmailExistsAsync(dto.Email))
        {
            return ServiceResult<UserDto>.Fail("此信箱已被註冊");
        }

        var newUser = _mapper.Map<User>(dto);
        newUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        await _authRepository.CreateUserAsync(newUser);

        var userDto = _mapper.Map<UserDto>(newUser);
        return ServiceResult<UserDto>.Ok(userDto);
    }

}
