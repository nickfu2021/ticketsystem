using AutoMapper;
using TicketSystemApi.Common;
using TicketSystemApi.Dtos;
using TicketSystemApi.Models;
using TicketSystemApi.Repositories;

namespace TicketSystemApi.Services;

public class CustomerService(ICustomerRepository customerRepository, IOrderRepository orderRepository, IMapper mapper) : ICustomerService
{
    private readonly ICustomerRepository _customerRepository = customerRepository;
    private readonly IOrderRepository _orderRepository = orderRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<ServiceResult<IEnumerable<CustomerDto>>> GetAllAsync()
    {
        var customers = await _customerRepository.GetAllAsync();
        if (customers == null)
        {
            return ServiceResult<IEnumerable<CustomerDto>>.Fail("找不到客戶");
        }

        var respDto = _mapper.Map<IEnumerable<CustomerDto>>(customers);
        return ServiceResult<IEnumerable<CustomerDto>>.Ok(respDto);
    }

    public async Task<ServiceResult<CustomerDto>> GetByIdAsync(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
        {
            return ServiceResult<CustomerDto>.Fail("找不到客戶");
        }

        var respDto = _mapper.Map<CustomerDto>(customer);
        return ServiceResult<CustomerDto>.Ok(respDto);
    }

    public async Task<ServiceResult<CustomerDto>> CreateAsync(CustomerCreateDto dto)
    {
        var customer = _mapper.Map<Customer>(dto);

        if (await _customerRepository.EmailExists(customer.Email))
        {
            return ServiceResult<CustomerDto>.Fail("此電子郵件已被註冊");
        }

        await _customerRepository.CreateAsync(customer);

        var respDto = _mapper.Map<CustomerDto>(customer);
        return ServiceResult<CustomerDto>.Ok(respDto);
    }

    public async Task<ServiceResult> UpdateAsync(int id, CustomerUpdateDto dto)
    {
        if (id != dto.Id)
        {
            return ServiceResult.Fail("提供的 ID 與 DTO 中的 ID 不符");
        }

        var customer = _mapper.Map<Customer>(dto);

        if (!await _customerRepository.ExistsAsync(id))
        {
            return ServiceResult<Customer>.Fail("此客戶不存在");
        }

        if (await _customerRepository.EmailExists(customer.Email))
        {
            return ServiceResult<Customer>.Fail("此電子郵件已被註冊");
        }

        await _customerRepository.UpdateAsync(customer);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
        {
            return ServiceResult.Fail("此客戶不存在");
        }

        var hasOrder = await _orderRepository.GetByCustomerIdAsync(id);
        if (hasOrder != null)
        {
            return ServiceResult.Fail("此客戶仍有訂單，無法刪除");
        }

        await _customerRepository.DeleteAsync(customer);
        return ServiceResult.Ok();
    }

}