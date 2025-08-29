namespace TicketSystemApi.Dtos;

public class RefreshResultDto
{
    public bool Ok { get; set; }
    public string? AccessToken { get; set; }
    public string? NewRefreshToken { get; set; }
    public string? Error { get; set; }
}
