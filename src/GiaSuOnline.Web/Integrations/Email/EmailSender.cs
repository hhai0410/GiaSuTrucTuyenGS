using System.Net;
using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
namespace GiaSuOnline.Web.Integrations.Email;
public interface IEmailSender { Task SendAsync(string to, string subject, string body); }
public class EmailSender(IConfiguration config, IWebHostEnvironment env) : IEmailSender
{
    public async Task SendAsync(string to, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(config["Email:From"] is { Length: > 0 } from ? from : "gs@localhost.test"));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };
        if (config["Email:Mode"] == "Pickup")
        {
            if (!env.IsDevelopment()) throw new InvalidOperationException("Pickup email chỉ được phép trong Development.");
            var dir = Path.Combine(env.ContentRootPath, "App_Data", "mail");
            Directory.CreateDirectory(dir);
            var name = Guid.NewGuid().ToString("N");
            await message.WriteToAsync(Path.Combine(dir, name + ".eml"));
            await File.WriteAllTextAsync(Path.Combine(dir, name + ".txt"), $"Người nhận: {to}\nTiêu đề: {subject}\n\n{body}");
            return;
        }
        var host = config["Email:Host"];
        if (string.IsNullOrWhiteSpace(host)) throw new InvalidOperationException("Chưa cấu hình máy chủ SMTP.");
        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(host, config.GetValue("Email:Port", 587),
            config.GetValue("Email:UseStartTls", true) ? SecureSocketOptions.StartTls : SecureSocketOptions.SslOnConnect);
        if (!string.IsNullOrWhiteSpace(config["Email:Username"]))
            await smtp.AuthenticateAsync(config["Email:Username"]!, config["Email:Password"] ?? throw new InvalidOperationException("Thiếu mật khẩu SMTP."));
        await smtp.SendAsync(message);
        await smtp.DisconnectAsync(true);
    }
}
