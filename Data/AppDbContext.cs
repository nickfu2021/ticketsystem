using Microsoft.EntityFrameworkCore;
using TicketSystemApi.Models;

namespace TicketSystemApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> option) : base(option) { }

    public DbSet<Events> Events => Set<Events>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
}
