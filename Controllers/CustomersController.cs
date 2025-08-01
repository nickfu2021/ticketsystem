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
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll()
    {
        var result = await _service.GetAllAsync();
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create([FromBody] CustomerCreateDto dto)
    {

        var result = await _service.CreateAsync(dto);
        if (!result.Success || result.Data == null)
        {
            return BadRequest(result);
        }

        var newCustomer = result.Data;

        return CreatedAtAction(nameof(GetById), new { id = newCustomer.Id }, newCustomer);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] CustomerUpdateDto dto)
    {

        var result = await _service.UpdateAsync(id, dto);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return NoContent();
    }
}