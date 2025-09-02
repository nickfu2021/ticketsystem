namespace TicketSystemApi.Dtos;

public class LoginResultDto
{
    public string AccessToken { get; set; } = "";
    public int ExpiresIn { get; set; } // 秒
    public string UserName { get; set; } = "";
}
