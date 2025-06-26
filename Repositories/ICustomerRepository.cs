using TicketSystemApi.Models;

namespace TicketSystemApi.Repositories;

public interface ICustomerRepository
{
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<Customer?> GetByIdAsync(int id);
    Task CreateAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(Customer customer);
    Task<bool> ExistsAsync(int id);
    Task<bool> HasOrderAsync(int customerId);
    Task<bool> EmailExists(string email);
}