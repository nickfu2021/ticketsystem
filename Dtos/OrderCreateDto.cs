namespace TicketSystemApi.Dtos;

public class OrderCreateDto
{
    public int CustomerId { get; set; }
    public int EventId { get; set; }
    public int Quantity { get; set; }

}