using TicketSystemApi.Common;
using TicketSystemApi.Models;
using TicketSystemApi.Repositories;

namespace TicketSystemApi.Services;

public class OrderService(IOrderRepository orderRepository, IEventRepository eventRepository) : IOrderService
{

    private readonly IOrderRepository _orderRepository = orderRepository;
    private readonly IEventRepository _eventRepository = eventRepository;

    public async Task<ServiceResult<IEnumerable<Order>>> GetByCustomerIdAsync(int customerId)
    {
        var orders = await _orderRepository.GetByCustomerIdAsync(customerId);
        if (orders == null)
        {
            return ServiceResult<IEnumerable<Order>>.Fail("此客戶無任何訂單");
        }

        return ServiceResult<IEnumerable<Order>>.Ok(orders);
    }

    public async Task<ServiceResult<Order>> CreateAsync(Order order)
    {
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
        return ServiceResult<Order>.Ok(order);
    }

    public async Task<ServiceResult> UpdateAsync(int id, Order order)
    {
        // check 1: 是否存在此訂單
        var existingOrder = await _orderRepository.GetByIdAsync(id);
        if (existingOrder == null)
        {
            return ServiceResult.Fail("此訂單不存在");
        }

        // check 2: 是否有足夠的票數
        var evt = await _eventRepository.GetByIdAsync(order.EventId);
        if (evt == null)
        {
            return ServiceResult.Fail("所選活動不存在");
        }

        // check 3: 確認更新後的票數不超過剩餘票數
        int sold = await _orderRepository.GetSoldCountAsync(order.EventId);
        int remaining = evt.TotalTickets - sold;

        if (order.Quantity > remaining)
        {
            return ServiceResult.Fail($"所選活動剩餘票數不足，僅剩 {remaining} 張");
        }

        await _orderRepository.UpdateAsync(order);
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