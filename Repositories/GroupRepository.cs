using Microsoft.EntityFrameworkCore;
using TicketSystemApi.Data;
using TicketSystemApi.Models;

namespace TicketSystemApi.Repositories;

public class GroupRepository(AppDbContext context) : IGroupRepository
{
    private readonly AppDbContext _context = context;

    public async Task<IEnumerable<Group>> GetAllAsync()
    {
        return await _context.Groups.ToListAsync();
    }

    public async Task<Group?> GetByIdAsync(int id)
    {
        return await _context.Groups.FindAsync(id);
    }

    public async Task CreateAsync(Group group)
    {
        _context.Groups.Add(group);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Group group)
    {
        _context.Groups.Update(group);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Group group)
    {
        _context.Groups.Remove(group);
        await _context.SaveChangesAsync();
    }
}