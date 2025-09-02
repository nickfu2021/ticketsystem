using TicketSystemApi.Common;
using TicketSystemApi.Dtos;

namespace TicketSystemApi.Services;

public interface IGroupService
{
    Task<ServiceResult<IEnumerable<GroupDto>>> GetAllAsync();
    Task<ServiceResult<GroupDto>> GetByIdAsync(int id);
    Task<ServiceResult<GroupDto>> CreateAsync(GroupCreateDto user);
    Task<ServiceResult<Unit>> UpdateAsync(int id, GroupUpdateDto user);
    Task<ServiceResult<Unit>> DeleteAsync(int id);

}