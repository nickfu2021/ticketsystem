namespace TicketSystemApi.Dtos;

public class LoginResultDto
{
    public string Token { get; set; } = "";
    public string UserName { get; set; } = "";
    public string Role { get; set; } = "";
}
