namespace TicketSystemApi.Services;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string htmlBody);
    Task SendSecurityAlertAsync(string toEmail, string? ip, string? userAgent);
}
