using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using BandHub.AuthService.Common;
using BandHub.AuthService.Configurations;
using BandHub.AuthService.Utils;

namespace BandHub.AuthService.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _email;
    private readonly SmtpSettings _smtp;
    private readonly MailAddress _from;

    public EmailService(IOptions<EmailSettings> emailOptions)
    {
        _email = emailOptions.Value;

        _smtp = _email.Mode.ToLower() switch
        {
            "gmail" => _email.Gmail,
            _ => _email.Smtp  // 預設 MailHog/Smtp
        };

        if (string.IsNullOrWhiteSpace(_smtp.From))
            throw new InvalidOperationException("Mail.From 未設定，請在設定檔或環境變數中提供寄件者信箱。");

        _from = new MailAddress(_smtp.From, _smtp.DisplayName ?? string.Empty);
    }

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(to))
            throw new ArgumentException("收件者信箱不可為空。", nameof(to));

        using var client = new SmtpClient(_smtp.Host, _smtp.Port)
        {
            EnableSsl = _smtp.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };

        // Gmail 需要帳密；MailHog 可以不需要
        if (!string.IsNullOrWhiteSpace(_smtp.Username))
        {
            client.Credentials = new NetworkCredential(_smtp.Username, _smtp.Password);
        }

        using var msg = new MailMessage
        {
            From = _from,
            Subject = subject ?? string.Empty,
            Body = htmlBody ?? string.Empty,
            IsBodyHtml = true,
        };
        msg.To.Add(new MailAddress(to));

        await client.SendMailAsync(msg);
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
