using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using TicketSystemApi.Configurations;
using TicketSystemApi.Services;

namespace TicketSystemApi.Services;

public class EmailService(IOptions<SmtpSettings> smtp) : IEmailService
{
    private readonly SmtpSettings _smtp = smtp.Value;

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        using var client = new SmtpClient(_smtp.Host, _smtp.Port)
        {
            Credentials = new NetworkCredential(_smtp.Username, _smtp.Password),
            EnableSsl = _smtp.EnableSsl
        };

        var message = new MailMessage
        {
            From = new MailAddress(_smtp.From, _smtp.DisplayName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        message.To.Add(to);

        await client.SendMailAsync(message);
    }

    public async Task SendSecurityAlertAsync(string toEmail, string? ip, string? userAgent)
    {
        var subject = "⚠️ 帳號安全警示通知";
        var htmlBody = $@"
            <p>親愛的使用者，</p>
            <p>系統偵測到您的帳號嘗試使用已失效的 Refresh Token，這通常代表您的帳號可能被入侵。</p>
            <p><b>來源 IP：</b> {ip ?? "未知"}</p>
            <p><b>裝置資訊：</b> {userAgent ?? "未知"}</p>
            <p>為了您的安全，我們已暫時鎖定帳號，請盡快聯繫客服或透過密碼重設功能恢復使用。</p>
            <br />
            <p>票券系統安全中心</p>";

        await SendAsync(toEmail, subject, htmlBody);
    }
}
