using Microsoft.AspNetCore.Mvc;
using TicketSystemApi.Dtos;
using TicketSystemApi.Models;
using TicketSystemApi.Services;
using AutoMapper;

namespace TicketSystemApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(IOrderService orderService, IMapper mapper) : ControllerBase
{
    private readonly IOrderService _orderService = orderService;
    private readonly IMapper _mapper = mapper;

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetById(int id)
    {
        var result = await _orderService.GetByIdAsync(id);
        if (!result.Success || result.Data == null)
        {
            return NotFound(new { message = result.ErrorMessage ?? "訂單不存在。" });
        }

        return Ok(_mapper.Map<OrderDto>(result.Data));
    }

    [HttpGet("customerId/{customerId}")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetByCustomerId(int customerId)
    {
        var result = await _orderService.GetByCustomerIdAsync(customerId);
        if (!result.Success)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        var dtoList = result.Data == null ? Enumerable.Empty<OrderDto>() : _mapper.Map<IEnumerable<OrderDto>>(result.Data);

        return Ok(dtoList);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create([FromBody] OrderCreateDto dto)
    {

        var result = await _orderService.CreateAsync(dto);
        if (!result.Success || result.Data == null)
        {
            return BadRequest(new { message = result.ErrorMessage ?? "建立訂單時發生未知錯誤。" });
        }

        var responseDto = _mapper.Map<OrderDto>(result.Data);

        return CreatedAtAction(nameof(GetById), new { id = responseDto.Id }, responseDto);

    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] OrderUpdateDto dto)
    {

        var result = await _orderService.UpdateAsync(id, dto);

        if (!result.Success)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _orderService.DeleteAsync(id);
        if (!result.Success)
        {
            return NotFound(new { message = result.ErrorMessage ?? "刪除訂單時發生未知錯誤。" });
        }

        return NoContent();
    }
}