using TicketSystemApi.Dtos;
using TicketSystemApi.Models;

namespace TicketSystemApi.Repositories;

public interface IGroupRepository
{
    Task<IEnumerable<Group>> GetAllAsync();
    Task<Group?> GetByIdAsync(int id);
    Task CreateAsync(Group group);
    Task UpdateAsync(Group group);
    Task DeleteAsync(Group group);
}