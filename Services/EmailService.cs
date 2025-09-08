using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using TicketSystemApi.Configurations;
using TicketSystemApi.Services;

namespace TicketSystemApi.Services;

public class MailOptions
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string From { get; set; } = "";           // 寄件者 email
    public string? DisplayName { get; set; }         // 寄件者顯示名稱
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

public class EmailService : IEmailService
{
    private readonly SmtpClient _smtp;
    private readonly MailAddress _from;

    public EmailService(IOptions<MailOptions> options)
    {
        var opt = options.Value;

        if (string.IsNullOrWhiteSpace(opt.From))
            throw new InvalidOperationException("Mail.From 未設定，請在設定檔或環境變數中提供寄件者信箱。");

        _from = new MailAddress(opt.From, opt.DisplayName ?? string.Empty);
        _smtp = new SmtpClient(opt.Host, opt.Port)
        {
            EnableSsl = opt.EnableSsl,
            Credentials = new NetworkCredential(opt.Username, opt.Password)
        };
    }

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(to))
            throw new ArgumentException("收件者信箱不可為空。", nameof(to));

        using var msg = new MailMessage
        {
            From = _from,
            Subject = subject ?? string.Empty,
            Body = htmlBody ?? string.Empty,
            IsBodyHtml = true,
        };
        msg.To.Add(new MailAddress(to));

        await _smtp.SendMailAsync(msg);
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
