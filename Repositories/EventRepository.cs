using Microsoft.EntityFrameworkCore;
using TicketSystemApi.Data;
using TicketSystemApi.Models;
using TicketSystemApi.Repositories;

public class EventRepository : IEventRepository
{
    private readonly AppDbContext _context;

    public EventRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Events>> GetAllAsync()
    {
        return await _context.Events.ToListAsync();
    }

    public async Task<Events?> GetByIdAsync(int id)
    {
        return await _context.Events.FindAsync(id);
    }

    public async Task AddAsync(Events evt)
    {
        _context.Events.Add(evt);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Events evt)
    {
        // 這段話的意思是：「這個 evt 不是我自己查的沒關係，我手動標記它為已修改」。
        _context.Entry(evt).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Events evt)
    {
        _context.Events.Remove(evt);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Events.AnyAsync(e => e.Id == id);
    }
}