using MailKit.Net.Smtp;
using MimeKit;
namespace GiaSuOnline.Web.Integrations.Email;
public interface IEmailSender { Task SendAsync(string to, string subject, string body); }
public class EmailSender(IConfiguration config, IWebHostEnvironment env) : IEmailSender
{
    public async Task SendAsync(string to, string subject, string body)
    {
        var mode = config["Email:Mode"] ?? "Smtp";
        if (mode != "Smtp" && mode != "Pickup") throw new InvalidOperationException("Email:Mode phải là Smtp hoặc Pickup.");
        var settings = mode == "Smtp" ? SmtpSettings.Load(config) : null;
        var message = new MimeMessage();
        message.From.Add(settings?.From ?? MailboxAddress.Parse("gs@localhost.test"));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };
        if (mode == "Pickup")
        {
            if (!env.IsDevelopment()) throw new InvalidOperationException("Pickup email chỉ được phép trong Development.");
            var dir = Path.Combine(env.ContentRootPath, "App_Data", "mail");
            Directory.CreateDirectory(dir);
            var name = Guid.NewGuid().ToString("N");
            await message.WriteToAsync(Path.Combine(dir, name + ".eml"));
            await File.WriteAllTextAsync(Path.Combine(dir, name + ".txt"), $"Người nhận: {to}\nTiêu đề: {subject}\n\n{body}");
            return;
        }
        using var smtp = new SmtpClient { Timeout = 15000 };
        void Progress(string text) { if (config.GetValue<bool>("SmtpCheck")) Console.WriteLine("SMTP: " + text); }
        Progress("Đang kết nối và thiết lập TLS...");
        await smtp.ConnectAsync(settings!.Host, settings.Port, settings.Security);
        Progress("TLS đã thiết lập. Đang xác thực tài khoản...");
        await smtp.AuthenticateAsync(settings.Username, settings.Password);
        Progress("Đã xác thực. Đang gửi thư thử...");
        await smtp.SendAsync(message);
        Progress("Máy chủ đã chấp nhận thư.");
        // Thư đã được chấp nhận; lỗi đóng kết nối sau đó không được báo là gửi thất bại.
        try { await smtp.DisconnectAsync(true); }
        catch (IOException) { }
        catch (MailKit.Net.Smtp.SmtpProtocolException) { }
    }
}
