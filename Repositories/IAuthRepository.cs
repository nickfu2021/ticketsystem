using TicketSystemApi.Models;

namespace TicketSystemApi.Repositories;

public interface IAuthRepository
{
    Task<User?> GetUserByEmailAsync(string email);
    Task CreateUserAsync(User user);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> IdNumberExistsAsync(string idNumber);
}
