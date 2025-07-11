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
            return NotFound();
        }
        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EventDto>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(new { message = result.ErrorMessage });
        }
        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<ActionResult<EventDto>> Create([FromBody] EventCreateDto dto)
    {
        var result = await _service.CreateAsync(dto);

        if (!result.Success || result.Data == null)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        var newEvent = result.Data;

        return CreatedAtAction(nameof(GetById), new { id = newEvent.Id }, newEvent);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvent(int id, [FromBody] EventUpdateDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);

        if (!result.Success)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);

        if (!result.Success)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        return NoContent();
    }
}