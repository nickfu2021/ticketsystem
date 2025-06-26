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
    public async Task<ActionResult<IEnumerable<Event>>> GetAll()
    {
        var result = await _service.GetAllAsync();
        if (!result.Success)
        {
            return NotFound();
        }
        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Event>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(new { message = result.ErrorMessage });
        }
        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<ActionResult<Event>> Create([FromBody] EventCreateDto dto)
    {
        var newEvent = new Event
        {
            Name = dto.Name,
            Location = dto.Location,
            EventDate = dto.EventDate,
            TotalTickets = dto.TotalTickets,
            Price = dto.Price
        };

        await _service.CreateAsync(newEvent);

        return CreatedAtAction(nameof(GetById), new { id = newEvent.Id }, newEvent);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvent(int id, [FromBody] EventUpdateDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest("路由ID與資料ID不相同");
        }

        var updated = new Event
        {
            Id = id,
            Name = dto.Name,
            Location = dto.Location,
            EventDate = dto.EventDate,
            TotalTickets = dto.TotalTickets,
            Price = dto.Price
        };

        var result = await _service.UpdateAsync(id, updated);
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