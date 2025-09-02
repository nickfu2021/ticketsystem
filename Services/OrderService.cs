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

    public async Task<ServiceResult<OrderDto>> GetByIdAsync(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            return ServiceResult<OrderDto>.Fail("此訂單不存在");
        }

        var respDto = _mapper.Map<OrderDto>(order);

        return ServiceResult<OrderDto>.Ok(respDto);
    }

    public async Task<ServiceResult<IEnumerable<OrderDto>>> GetByCustomerIdAsync(int customerId)
    {
        var orders = await _orderRepository.GetByCustomerIdAsync(customerId);
        if (!orders.Any())
        {
            return ServiceResult<IEnumerable<OrderDto>>.Fail("此客戶無任何訂單");
        }

        var respDto = _mapper.Map<IEnumerable<OrderDto>>(orders);
        return ServiceResult<IEnumerable<OrderDto>>.Ok(respDto);
    }

    public async Task<ServiceResult<OrderDto>> CreateAsync(OrderCreateDto dto)
    {
        var order = _mapper.Map<Order>(dto);
        order.Ordertime = DateTime.UtcNow;

        // check 1: 確認所選活動存在
        var evt = await _eventRepository.GetByIdAsync(order.EventId);
        if (evt == null)
        {
            return ServiceResult<OrderDto>.Fail("所選活動不存在");
        }

        // check 2: 確認客戶是否已經有此活動的訂單
        if (await _orderRepository.HasOrderAsync(order.CustomerId, order.EventId))
        {
            return ServiceResult<OrderDto>.Fail("客戶已經有此活動的訂單，無法重複訂購");
        }

        // check 3: 是否有足夠的票數
        int sold = await _orderRepository.GetSoldCountAsync(order.EventId);
        int remaining = evt.TotalTickets - sold;
        if (order.Quantity > remaining)
        {
            return ServiceResult<OrderDto>.Fail($"所選活動剩餘票數不足，僅剩 {remaining} 張");
        }

        var createOrder = await _orderRepository.CreateAsync(order);

        var result = await _orderRepository.GetByIdAsync(createOrder.Id);
        if (result == null)
        {
            return ServiceResult<OrderDto>.Fail("訂單建立後查詢失敗");
        }

        var respDto = _mapper.Map<OrderDto>(result);

        return ServiceResult<OrderDto>.Ok(respDto);
    }

    public async Task<ServiceResult<Unit>> UpdateAsync(int id, OrderUpdateDto dto)
    {
        // check 1: 檢查路由ID與資料ID是否一致
        if (id != dto.Id)
        {
            return ServiceResult<Unit>.Fail("路由ID與資料ID不一致");
        }

        var order = _mapper.Map<Order>(dto);

        // check 2: 是否存在此訂單
        var existingOrder = await _orderRepository.GetByIdAsync(order.Id);
        if (existingOrder == null)
        {
            return ServiceResult<Unit>.Fail("此訂單不存在");
        }

        // check 2: 確認活動存不存在
        var evt = await _eventRepository.GetByIdAsync(existingOrder.EventId);
        if (evt == null)
        {
            return ServiceResult<Unit>.Fail("所選活動不存在");
        }

        // check 3: 確認更新後的票數不超過剩餘票數
        int sold = await _orderRepository.GetSoldCountAsync(existingOrder.EventId);
        int remaining = evt.TotalTickets - sold;

        if (order.Quantity > remaining)
        {
            return ServiceResult<Unit>.Fail($"所選活動剩餘票數不足，僅剩 {remaining} 張");
        }

        existingOrder.Quantity = order.Quantity;

        await _orderRepository.UpdateAsync(existingOrder);
        return ServiceResult<Unit>.Ok(Unit.Value);
    }

    public async Task<ServiceResult<Unit>> DeleteAsync(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            return ServiceResult<Unit>.Fail("此訂單不存在");
        }

        await _orderRepository.DeleteAsync(order);
        return ServiceResult<Unit>.Ok(Unit.Value);
    }
}