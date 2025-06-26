using TicketSystemApi.Common;
using TicketSystemApi.Models;
using TicketSystemApi.Repositories;

namespace TicketSystemApi.Services;

public class CustomerService(ICustomerRepository repository) : ICustomerService
{
    private readonly ICustomerRepository _repository = repository;

    public async Task<ServiceResult<IEnumerable<Customer>>> GetAllAsync()
    {
        var customers = await _repository.GetAllAsync();
        if (customers == null)
        {
            return ServiceResult<IEnumerable<Customer>>.Fail("找不到客戶");
        }
        return ServiceResult<IEnumerable<Customer>>.Ok(customers);
    }

    public async Task<ServiceResult<Customer>> GetByIdAsync(int id)
    {
        var customer = await _repository.GetByIdAsync(id);
        if (customer == null)
        {
            return ServiceResult<Customer>.Fail("找不到客戶");
        }
        return ServiceResult<Customer>.Ok(customer);
    }

    public async Task<ServiceResult<Customer>> CreateAsync(Customer customer)
    {
        if (await _repository.EmailExists(customer.Email))
        {
            return ServiceResult<Customer>.Fail("此電子郵件已被註冊");
        }

        await _repository.CreateAsync(customer);
        return ServiceResult<Customer>.Ok(customer);
    }

    public async Task<ServiceResult> UpdateAsync(int id, Customer customer)
    {
        if (!await _repository.ExistsAsync(id))
        {
            return ServiceResult<Customer>.Fail("此客戶不存在");
        }

        if (await _repository.EmailExists(customer.Email))
        {
            return ServiceResult<Customer>.Fail("此電子郵件已被註冊");
        }

        await _repository.UpdateAsync(customer);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var customer = await _repository.GetByIdAsync(id);
        if (customer == null)
        {
            return ServiceResult.Fail("此客戶不存在");
        }

        if (await _repository.HasOrderAsync(id))
        {
            return ServiceResult.Fail("此客戶仍有訂單，無法刪除");
        }

        await _repository.DeleteAsync(customer);
        return ServiceResult.Ok();
    }

}