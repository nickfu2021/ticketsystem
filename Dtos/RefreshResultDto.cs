namespace TicketSystemApi.Dtos;

public class RefreshResultDto
{
    public string? AccessToken { get; set; }
    public int ExpiresIn { get; set; }
}
