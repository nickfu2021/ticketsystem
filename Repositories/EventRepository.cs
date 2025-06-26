using Microsoft.EntityFrameworkCore;
using TicketSystemApi.Data;
using TicketSystemApi.Models;
using TicketSystemApi.Repositories;

namespace TicketSystemApi.Repositories;
public class EventRepository(AppDbContext context) : IEventRepository
{
    private readonly AppDbContext _context = context;

    public async Task<IEnumerable<Event>> GetAllAsync()
    {
        return await _context.Events.ToListAsync();
    }

    public async Task<Event?> GetByIdAsync(int id)
    {
        return await _context.Events.FindAsync(id);
    }

    public async Task AddAsync(Event evt)
    {
        _context.Events.Add(evt);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Event evt)
    {
        // 情境一:所有欄位都更新
        // 方法一
        _context.Events.Update(evt);

        // 方法二
        // 這段話的意思是：「這個 evt 不是我自己查的沒關係，我手動標記它為已修改」。
        //_context.Entry(evt).State = EntityState.Modified;

        // 情境二:只要更新特定欄位，Services 層也要改
        // var evt = new Events { Id = id };
        // _context.Attach(evt);
        // evt.Price = 1200;
        // _context.Entry(evt).Property(e => e.Price).IsModified = true;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Event evt)
    {
        _context.Events.Remove(evt);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Events.AnyAsync(e => e.Id == id);
    }
}