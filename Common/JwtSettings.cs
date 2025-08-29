namespace TicketSystemApi.Common;

public class JwtSettings
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public int ExpiresMinutes { get; set; } = 60;
    public int RefreshTokenDays { get; set; } = 14;
    public string RefreshTokenPepper { get; set; } = "";
}