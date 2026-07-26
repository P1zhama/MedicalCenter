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

        var body =
            "<p>Welcome to Medical Center.</p>" +
            "<p>Please confirm your email to finish creating your account:</p>" +
            $"<p><a href=\"{link}\">Confirm email</a></p>";

        await SendAsync(email, "Confirm your email", body, cancellationToken);

        _logger.LogInformation("Confirmation email sent to {Email}", email);
    }

    public async Task SendWorkerCredentialsAsync(string email, string temporaryPassword, CancellationToken cancellationToken = default)
    {
        var body =
            "<p>An account has been created for you at Medical Center.</p>" +
            $"<p>Login: {email}</p>" +
            $"<p>Temporary password: {temporaryPassword}</p>" +
            "<p>Please sign in and change your password.</p>";

        await SendAsync(email, "Your Medical Center account", body, cancellationToken);

        _logger.LogInformation("Credentials email sent to {Email}", email);
    }

    private async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        var secureSocketOptions = _settings.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;

        using var client = new SmtpClient();

        await client.ConnectAsync(_settings.Server, _settings.Port, secureSocketOptions, cancellationToken);
        await client.AuthenticateAsync(_settings.SenderEmail, _settings.Password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
