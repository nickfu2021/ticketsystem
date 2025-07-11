using Microsoft.EntityFrameworkCore;
using TicketSystemApi.Data;
using TicketSystemApi.Models;

namespace TicketSystemApi.Repositories;

public class OrderRepository(AppDbContext context) : IOrderRepository
{
    private readonly AppDbContext _context = context;

    public async Task<bool> HasOrderForEventAsync(int eventId)
    {
        return await _context.Orders.AnyAsync(o => o.EventId == eventId);
    }

    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(int customer_id)
    {
        return await _context.Orders
            .Include(o => o.Event)
            .Include(o => o.Customer)
            .Where(o => o.CustomerId == customer_id).ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        /*
            FindAsync 是 EF Core 特別的方法，會直接用主鍵查詢，並且繞過你定義的 LINQ 條件與 Include
            所以即使你加了 .Include(...)，它也不會作用，結果 Event 和 Customer 仍然是 null。
        */
        return await _context.Orders
            .Include(o => o.Event)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order> CreateAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Order order)
    {
        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Orders.AnyAsync(o => o.Id == id);
    }

    public async Task<int> GetSoldCountAsync(int eventId)
    {
        return await _context.Orders.Where(o => o.EventId == eventId).SumAsync(o => o.Quantity);
    }

    public async Task<bool> HasOrderAsync(int customerId, int eventId)
    {
        return await _context.Orders.AnyAsync(o => o.CustomerId == customerId && o.EventId == eventId);
    }
}