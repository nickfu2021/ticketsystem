namespace TicketSystemApi.Dtos;

public class EventDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public int TotalTickets { get; set; }
    public decimal Price { get; set; }

}