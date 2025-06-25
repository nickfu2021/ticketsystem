using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSystemApi.Data;
using TicketSystemApi.Models;

namespace TicketSystemApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly AppDbContext _context;

    public EventsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Events>>> GetEvents()
    {
        return await _context.Events.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Events>> GetEvent(int id)
    {
        var evt = await _context.Events.FindAsync(id);
        if (evt == null)
        {
            return NotFound();
        }
        return evt;
    }

    [HttpPost]
    public async Task<ActionResult<Events>> AddEvent(Events evt)
    {

        evt.EventDate = evt.EventDate.Date;

        _context.Events.Add(evt);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEvent), new { id = evt.Id }, evt);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvent(int id, Events evt)
    {
        if (id != evt.Id)
        {
            return BadRequest();
        }

        _context.Entry(evt).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Events.Any(e => e.Id == id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }

        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var evt = await _context.Events.FindAsync(id);
        if (evt == null)
        {
            return NotFound();
        }

        _context.Events.Remove(evt);
        await _context.SaveChangesAsync();

        return NoContent();

    }
}