using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSystemApi.Data;
using TicketSystemApi.Dtos;
using TicketSystemApi.Models;
using TicketSystemApi.Services;

namespace TicketSystemApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventService _service;

    public EventsController(IEventService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Events>>> GetAll()
    {
        var events = await _service.GetAllAsync();
        return Ok(events);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Events>> GetById(int id)
    {
        var evt = await _service.GetByIdAsync(id);
        if (evt == null)
        {
            return NotFound();
        }
        return evt;
    }

    [HttpPost]
    public async Task<ActionResult<Events>> Create([FromBody] EventCreateDto dto)
    {
        var newEvent = new Events
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
        var updated = new Events
        {
            Id = id,
            Name = dto.Name,
            Location = dto.Location,
            EventDate = dto.EventDate,
            TotalTickets = dto.TotalTickets,
            Price = dto.Price
        };

        var result = await _service.UpdateAsync(id, updated);
        if (!result) return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result) return NotFound();

        return NoContent();
    }
}