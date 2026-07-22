using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Authorization.Infrastructure.Notifications;

public sealed class MailKitEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;
    private readonly ILogger<MailKitEmailSender> _logger;

    public MailKitEmailSender(IOptions<EmailSettings> settings, ILogger<MailKitEmailSender> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailConfirmationAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        var link = $"{_settings.ClientAppBaseUrl}/confirm-email?token={Uri.EscapeDataString(token)}";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = "Confirm your email";
        message.Body = new BodyBuilder
        {
            HtmlBody =
                $"<p>Welcome to Medical Center.</p>" +
                $"<p>Please confirm your email to finish creating your account:</p>" +
                $"<p><a href=\"{link}\">Confirm email</a></p>"
        }.ToMessageBody();

        var secureSocketOptions = _settings.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;

        using var client = new SmtpClient();

        await client.ConnectAsync(_settings.Server, _settings.Port, secureSocketOptions, cancellationToken);
        await client.AuthenticateAsync(_settings.SenderEmail, _settings.Password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation("Confirmation email sent to {Email}", email);
    }
}
