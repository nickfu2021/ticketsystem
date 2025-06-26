using Microsoft.AspNetCore.Mvc;
using TicketSystemApi.Dtos;
using TicketSystemApi.Models;
using TicketSystemApi.Services;

namespace TicketSystemApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController(ICustomerService service) : ControllerBase
{
    private readonly ICustomerService _service = service;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Customer>>> GetAll()
    {
        var result = await _service.GetAllAsync();
        if (!result.Success)
        {
            return NotFound(new { message = result.ErrorMessage });
        }
        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Customer>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(new { message = result.ErrorMessage });
        }
        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<ActionResult<Customer>> Create([FromBody] CustomerCreateDto dto)
    {
        var newCustomer = new Customer
        {
            Name = dto.Name,
            Email = dto.Email
        };

        var result = await _service.CreateAsync(newCustomer);
        if (!result.Success)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return CreatedAtAction(nameof(GetById), new { id = newCustomer.Id }, newCustomer);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] CustomerUpdateDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest(new { message = "路由ID與資料ID不一致" });
        }

        var updated = new Customer
        {
            Id = dto.Id,
            Name = dto.Name,
            Email = dto.Email
        };

        var result = await _service.UpdateAsync(id, updated);
        if (!result.Success)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result.Success)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        return NoContent();
    }
}