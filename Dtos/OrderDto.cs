namespace TicketSystemApi.Dtos;

public class OrderDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int CustomerId { get; set; }
    public int Quantity { get; set; }
    public DateTime Ordertime { get; set; }

}
