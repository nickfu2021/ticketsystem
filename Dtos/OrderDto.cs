namespace TicketSystemApi.Dtos;

public class OrderDto
{
    public int Id { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime Ordertime { get; set; }
}