namespace TicketSystemApi.Dtos;

public class LoginResultDto
{
    public string AccessToken { get; set; } = "";
    public int ExpiresIn { get; set; } // 秒
    public string RefreshToken { get; set; } = ""; // 明文（只回傳給用戶，不入庫）
    public string UserName { get; set; } = "";
}
