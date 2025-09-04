namespace TicketSystemApi.Dtos;

public class RefreshResultDto
{
    public bool Ok { get; set; }
    public string? AccessToken { get; set; }
    public string? Error { get; set; }
    public int ExpiresIn { get; set; }
}
