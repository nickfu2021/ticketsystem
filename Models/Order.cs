using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSystemApi.Models;
[Table("orders")]
public class Order
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("customer_id")]
    public int CustomerId { get; set; }
    [Column("event_id")]
    public int EventId { get; set; }
    [Column("quantity")]
    public int Quantity { get; set; }
    [Column("order_time")]
    public DateTime Ordertime { get; set; } = DateTime.Now;

    public Event Event { get; set; } = null!;
    public Customer Customer { get; set; } = null!;

}