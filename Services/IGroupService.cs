using TicketSystemApi.Common;
using TicketSystemApi.Dtos;

namespace TicketSystemApi.Services;

public interface IGroupService
{
    Task<ServiceResult<IEnumerable<GroupDto>>> GetAllAsync();
    Task<ServiceResult<GroupDto>> GetByIdAsync(int id);
    Task<ServiceResult<GroupDto>> CreateAsync(GroupCreateDto user);
    Task<ServiceResult> UpdateAsync(int id, GroupUpdateDto user);
    Task<ServiceResult> DeleteAsync(int id);

}