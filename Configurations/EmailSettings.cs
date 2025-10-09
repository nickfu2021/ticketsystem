namespace BandHub.AuthService.Configurations;

public class EmailSettings
{
    public string Mode { get; set; } = "Smtp";
    public string VerifyBaseUrl { get; set; } = string.Empty;


    // SMTP 子設定
    public SmtpSettings Smtp { get; set; } = new();
    public SmtpSettings Gmail { get; set; } = new();
    
}

public class SmtpSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public string From { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
