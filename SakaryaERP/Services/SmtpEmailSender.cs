using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace SakaryaERP.Services;

// SMTP bilgileri (Smtp:User, Smtp:Password) appsettings.json'da tutulmaz,
// user-secrets/.env ile ayarlanır (bkz. ConnectionStrings ile aynı kural).
public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public SmtpEmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
    {
        var host = _configuration["Smtp:Host"] ?? throw new InvalidOperationException("Smtp:Host yapılandırılmamış.");
        var port = int.Parse(_configuration["Smtp:Port"] ?? "587");
        var from = _configuration["Smtp:From"] ?? "noreply@sakaryaerp.com";
        var kullanici = _configuration["Smtp:User"];
        var sifre = _configuration["Smtp:Password"];

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(from));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlMessage };

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, SecureSocketOptions.Auto);
        if (!string.IsNullOrWhiteSpace(kullanici))
            await client.AuthenticateAsync(kullanici, sifre ?? "");
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
