namespace TicketSystemApi.Dtos;

public class TokenDto
{
    public string AccessToken { get; set; } = default!;
    public int ExpiresIn { get; set; }
}
