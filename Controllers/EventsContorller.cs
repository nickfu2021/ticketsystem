using Microsoft.AspNetCore.Mvc;
using TicketSystemApi.Dtos;
using TicketSystemApi.Models;
using TicketSystemApi.Services;

namespace TicketSystemApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController(IEventService service) : ControllerBase
{
    private readonly IEventService _service = service;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetAll()
    {
        var result = await _service.GetAllAsync();
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EventDto>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<ActionResult<EventDto>> Create([FromBody] EventCreateDto dto)
    {
        var result = await _service.CreateAsync(dto);

        if (!result.Success || result.Data == null)
        {
            return BadRequest(result);
        }

        var newEvent = result.Data;

        return CreatedAtAction(nameof(GetById), new { id = newEvent.Id }, newEvent);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] EventUpdateDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return NoContent();
    }
}