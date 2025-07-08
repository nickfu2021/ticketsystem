using AutoMapper;
using TicketSystemApi.Common;
using TicketSystemApi.Dtos;
using TicketSystemApi.Models;
using TicketSystemApi.Repositories;

namespace TicketSystemApi.Services;

public class OrderService(IOrderRepository orderRepository, IEventRepository eventRepository, IMapper mapper) : IOrderService
{

    private readonly IOrderRepository _orderRepository = orderRepository;
    private readonly IEventRepository _eventRepository = eventRepository;

    private readonly IMapper _mapper = mapper;

    public async Task<ServiceResult<Order>> GetByIdAsync(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            return ServiceResult<Order>.Fail("此訂單不存在");
        }

        return ServiceResult<Order>.Ok(order);
    }

    public async Task<ServiceResult<IEnumerable<Order>>> GetByCustomerIdAsync(int customerId)
    {
        var orders = await _orderRepository.GetByCustomerIdAsync(customerId);
        if (!orders.Any())
        {
            return ServiceResult<IEnumerable<Order>>.Fail("此客戶無任何訂單");
        }

        return ServiceResult<IEnumerable<Order>>.Ok(orders);
    }

    public async Task<ServiceResult<Order>> CreateAsync(OrderCreateDto dto)
    {
        var order = _mapper.Map<Order>(dto);
        order.Ordertime = DateTime.UtcNow;

        // check 1: 確認所選活動存在
        var evt = await _eventRepository.GetByIdAsync(order.EventId);
        if (evt == null)
        {
            return ServiceResult<Order>.Fail("所選活動不存在");
        }

        // check 2: 是否有足夠的票數
        int sold = await _orderRepository.GetSoldCountAsync(order.EventId);
        int remaining = evt.TotalTickets - sold;
        if (order.Quantity > remaining)
        {
            return ServiceResult<Order>.Fail($"所選活動剩餘票數不足，僅剩 {remaining} 張");
        }

        await _orderRepository.CreateAsync(order);

        var fullOrder = await _orderRepository.GetByIdAsync(order.Id);
        if (fullOrder == null)
        {
            return ServiceResult<Order>.Fail("訂單建立後查詢失敗");
        }
        return ServiceResult<Order>.Ok(fullOrder);
    }

    public async Task<ServiceResult> UpdateAsync(int id, OrderUpdateDto dto)
    {
        // check 1: 檢查路由ID與資料ID是否一致
        if (id != dto.Id)
        {
            return ServiceResult.Fail("路由ID與資料ID不一致");
        }

        var order = _mapper.Map<Order>(dto);

        // check 2: 是否存在此訂單
        var existingOrder = await _orderRepository.GetByIdAsync(order.Id);
        if (existingOrder == null)
        {
            return ServiceResult.Fail("此訂單不存在");
        }

        // check 2: 確認活動存不存在
        var evt = await _eventRepository.GetByIdAsync(existingOrder.EventId);
        if (evt == null)
        {
            return ServiceResult.Fail("所選活動不存在");
        }

        // check 3: 確認更新後的票數不超過剩餘票數
        int sold = await _orderRepository.GetSoldCountAsync(existingOrder.EventId);
        int remaining = evt.TotalTickets - sold;

        if (order.Quantity > remaining)
        {
            return ServiceResult.Fail($"所選活動剩餘票數不足，僅剩 {remaining} 張");
        }

        existingOrder.Quantity = order.Quantity;

        await _orderRepository.UpdateAsync(existingOrder);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            return ServiceResult.Fail("此訂單不存在");
        }

        await _orderRepository.DeleteAsync(order);
        return ServiceResult.Ok();
    }
}