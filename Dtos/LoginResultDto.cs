namespace TicketSystemApi.Dtos;

public class LoginResultDto
{
    public bool Ok { get; set; }
    public string? Error { get; set; }
    public string AccessToken { get; set; } = "";
    public int ExpiresIn { get; set; } // 秒
    public string? UserName { get; set; }
}
