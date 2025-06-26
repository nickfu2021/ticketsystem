using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSystemApi.Models;

[Table("events")]
public class Event
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("name")]
    public string Name { get; set; } = null!;
    [Column("location")]
    public string Location { get; set; } = null!;
    [Column("event_date", TypeName = "date")]
    public DateTime EventDate { get; set; }
    [Column("total_tickets")]
    public int TotalTickets { get; set; }
    [Column("price")]
    public decimal Price { get; set; }

    public List<Order> Orders { get; set; } = new();
}