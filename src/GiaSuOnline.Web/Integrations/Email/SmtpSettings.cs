using MailKit.Security;
using MimeKit;
using System.ComponentModel.DataAnnotations;

namespace GiaSuOnline.Web.Integrations.Email;

public record SmtpSettings(string Host, int Port, string Username, string Password,
    MailboxAddress From, SecureSocketOptions Security)
{
    public static SmtpSettings Load(IConfiguration config)
    {
        string Required(string key) => string.IsNullOrWhiteSpace(config[key])
            ? throw new InvalidOperationException($"Thiếu cấu hình {key}.") : config[key]!.Trim();
        var host = Required("Email:Host");
        var port = config.GetValue("Email:Port", 587);
        if (Uri.CheckHostName(host) == UriHostNameType.Unknown || host.Contains('/') || port is < 1 or > 65535)
            throw new InvalidOperationException("Máy chủ hoặc cổng SMTP không hợp lệ.");
        var username = Required("Email:Username");
        var password = config["Email:Password"];
        if (string.IsNullOrWhiteSpace(password)) throw new InvalidOperationException("Thiếu mật khẩu ứng dụng SMTP.");
        var from = Required("Email:From");
        if (!MailboxAddress.TryParse(from, out var mailbox) || !new EmailAddressAttribute().IsValid(mailbox.Address))
            throw new InvalidOperationException("Địa chỉ gửi email không hợp lệ.");
        var security = config.GetValue("Email:UseStartTls", true) ? SecureSocketOptions.StartTls : SecureSocketOptions.SslOnConnect;
        return new(host, port, username, password, mailbox, security);
    }
    // Tránh vô tình đưa mật khẩu vào log nếu có người in đối tượng cấu hình.
    public override string ToString() => $"SMTP {Host}:{Port}; {Security}; credentials hidden";
}
