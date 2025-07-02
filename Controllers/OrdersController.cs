using Microsoft.AspNetCore.Mvc;
using TicketSystemApi.Dtos;
using TicketSystemApi.Models;
using TicketSystemApi.Services;

namespace TicketSystemApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    private readonly IOrderService _orderService = orderService;

    [HttpGet("{customerId}")]
    public async Task<ActionResult<IEnumerable<Order>>> GetByCustomerId(int customerId)
    {
        var result = await _orderService.GetByCustomerIdAsync(customerId);
        if (!result.Success)
        {
            return NotFound(new { message = result.ErrorMessage });
        }
        if (result.Data == null)
        {
            return Ok(Enumerable.Empty<OrderDto>());
        }
        var dtoList = result.Data.Select(o => new OrderDto
        {
            Id = o.Id,
            EventId = o.EventId,
            CustomerId = o.CustomerId,
            Quantity = o.Quantity,
            Ordertime = o.Ordertime
        });
        return Ok(dtoList);
    }

    [HttpPost]
    public async Task<ActionResult<Order>> Create([FromBody] OrderCreateDto orderCreateDto)
    {

        var newOrder = new Order
        {
            EventId = orderCreateDto.EventId,
            CustomerId = orderCreateDto.CustomerId,
            Quantity = orderCreateDto.Quantity,
            Ordertime = DateTime.UtcNow
        };

        var result = await _orderService.CreateAsync(newOrder);
        if (!result.Success)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        if (result.Data == null)
        {
            return StatusCode(500, new { message = "建立訂單時發生未知錯誤。" });
        }
        var dto = new OrderDto
        {
            Id = result.Data.Id,
            EventId = result.Data.EventId,
            CustomerId = result.Data.CustomerId,
            Quantity = result.Data.Quantity,
            Ordertime = result.Data.Ordertime
        };

        return CreatedAtAction(nameof(GetByCustomerId), new { customerId = newOrder.CustomerId }, dto);

    }
}