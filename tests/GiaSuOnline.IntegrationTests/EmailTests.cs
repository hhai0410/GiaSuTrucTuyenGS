using GiaSuOnline.Web.Integrations.Email;
using MailKit.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Xunit;

namespace GiaSuOnline.IntegrationTests;

public class EmailTests
{
    private static Dictionary<string, string?> Settings() => new()
    {
        ["Email:Mode"] = "Smtp", ["Email:Host"] = "smtp.gmail.com", ["Email:Port"] = "587",
        ["Email:From"] = "sender@example.test", ["Email:Username"] = "sender@example.test",
        ["Email:Password"] = "test-only-not-a-real-password", ["Email:UseStartTls"] = "true"
    };
    [Fact] public void GmailConfigurationRequiresTlsAndHidesPasswordInDiagnosticString()
    {
        var settings = SmtpSettings.Load(new ConfigurationBuilder().AddInMemoryCollection(Settings()).Build());
        Assert.Equal(SecureSocketOptions.StartTls, settings.Security);
        Assert.Equal(587, settings.Port);
        Assert.DoesNotContain(settings.Password, settings.ToString());
    }
    [Theory]
    [InlineData("Email:Password", "")]
    [InlineData("Email:From", "invalid")]
    [InlineData("Email:Host", "https://smtp.gmail.com")]
    [InlineData("Email:Port", "70000")]
    public void InvalidConfigurationRejectedBeforeConnection(string key, string value)
    {
        var values = Settings(); values[key] = value;
        Assert.Throws<InvalidOperationException>(() => SmtpSettings.Load(new ConfigurationBuilder().AddInMemoryCollection(values).Build()));
    }
    [Fact] public async Task PickupCannotBeUsedOutsideDevelopment()
    {
        var values = Settings(); values["Email:Mode"] = "Pickup";
        var sender = new EmailSender(new ConfigurationBuilder().AddInMemoryCollection(values).Build(), new TestEnvironment { EnvironmentName = "Production" });
        await Assert.ThrowsAsync<InvalidOperationException>(() => sender.SendAsync("receiver@example.test", "test", "test"));
    }
    [Fact] public async Task MisspelledModeDoesNotSendOverNetwork()
    {
        var values = Settings(); values["Email:Mode"] = "Pickupp";
        var sender = new EmailSender(new ConfigurationBuilder().AddInMemoryCollection(values).Build(), new TestEnvironment());
        await Assert.ThrowsAsync<InvalidOperationException>(() => sender.SendAsync("receiver@example.test", "test", "test"));
    }
    private class TestEnvironment : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "Tests";
        public string WebRootPath { get; set; } = "";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = "";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
