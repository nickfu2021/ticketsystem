using TicketSystemApi.Dtos;
using TicketSystemApi.Models;

namespace TicketSystemApi.Repositories;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);
    Task<bool> EmailExists(string email);
}