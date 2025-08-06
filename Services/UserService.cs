using AutoMapper;
using TicketSystemApi.Common;
using TicketSystemApi.Dtos;
using TicketSystemApi.Models;
using TicketSystemApi.Repositories;

namespace TicketSystemApi.Services;

public class UserService(IUserRepository userRepository, IMapper mapper) : IUserService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IMapper _mapper = mapper;
    public async Task<ServiceResult<IEnumerable<UserDto>>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        if (users == null)
        {
            return ServiceResult<IEnumerable<UserDto>>.Fail("找不到使用者");
        }

        var respDto = _mapper.Map<IEnumerable<UserDto>>(users);
        return ServiceResult<IEnumerable<UserDto>>.Ok(respDto);
    }

    public async Task<ServiceResult<UserDto>> GetByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return ServiceResult<UserDto>.Fail("找不到使用者");
        }

        var respDto = _mapper.Map<UserDto>(user);
        return ServiceResult<UserDto>.Ok(respDto);
    }

    public async Task<ServiceResult<UserDto>> CreateAsync(UserCreateDto dto)
    {
        var user = _mapper.Map<User>(dto);

        if (await _userRepository.EmailExists(user.Email))
        {
            return ServiceResult<UserDto>.Fail("此信箱已被註冊");
        }

        await _userRepository.CreateAsync(user);
        var respDto = _mapper.Map<UserDto>(user);

        return ServiceResult<UserDto>.Ok(respDto);

    }

    public async Task<ServiceResult> UpdateAsync(int id, UserUpdateDto dto)
    {
        if (id != dto.Id)
        {
            return ServiceResult.Fail("提供的 ID 與 DTO 中的 ID 不符");
        }

        var existingUser = await _userRepository.GetByIdAsync(id);
        if (existingUser == null)
        {
            return ServiceResult.Fail("使用者不存在");
        }
        
        if (await _userRepository.EmailExists(dto.Email))
        {
            return ServiceResult.Fail("信箱已被註冊");
        }

        _mapper.Map(dto, existingUser);

        await _userRepository.UpdateAsync(existingUser);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return ServiceResult.Fail("此客戶不存在");
        }

        await _userRepository.DeleteAsync(user);
        return ServiceResult.Ok();
    }
}