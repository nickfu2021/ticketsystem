namespace TicketSystemApi.Dtos;

public class EventCreateDto
{
    public string Name { get; set; } = null!;
    public string Location { get; set; } = null!;
    public DateTime EventDate { get; set; }
    public int TotalTickets { get; set; }
    public decimal Price { get; set; }
}
