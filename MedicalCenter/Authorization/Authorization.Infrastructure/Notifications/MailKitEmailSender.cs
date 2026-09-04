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

    public async Task SendAppointmentNotificationAsync(
        string email,
        AppointmentNotification notification,
        CancellationToken cancellationToken = default)
    {
        var when = notification.AppointmentStart.ToString("dd.MM.yyyy HH:mm");

        var (subject, headline) = notification.Kind switch
        {
            AppointmentNotificationKinds.Approved =>
                ("Your appointment is confirmed", "Your appointment has been confirmed."),
            AppointmentNotificationKinds.Cancelled =>
                ("Your appointment is cancelled", "Your appointment has been cancelled."),
            AppointmentNotificationKinds.Rescheduled =>
                ("Your appointment is rescheduled", "Your appointment has been moved to a new time."),
            AppointmentNotificationKinds.Reminder =>
                ("Reminder: your appointment is tomorrow", "This is a reminder about your appointment tomorrow."),
            _ => ("Your appointment has changed", "Your appointment has changed.")
        };

        var body =
            $"<p>{headline}</p>" +
            $"<p>Patient: {notification.PatientFullName}<br/>" +
            $"Service: {notification.ServiceName}<br/>" +
            $"Doctor: {notification.DoctorFullName}<br/>" +
            $"Date and time: {when}</p>";

        await SendAsync(email, subject, body, cancellationToken);

        _logger.LogInformation("Appointment {Kind} notification sent to {Email}", notification.Kind, email);
    }

    public async Task SendAppointmentResultAsync(
        string email,
        string fileName,
        byte[] content,
        CancellationToken cancellationToken = default)
    {
        var body =
            "<p>Your appointment result is attached to this message.</p>" +
            "<p>Medical Center</p>";

        var builder = new BodyBuilder { HtmlBody = body };

        builder.Attachments.Add(fileName, content, ContentType.Parse("application/pdf"));

        await SendAsync(email, "Your appointment result", builder.ToMessageBody(), cancellationToken);

        _logger.LogInformation("Appointment result sent to {Email}", email);
    }

    private Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken)
        => SendAsync(toEmail, subject, new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody(), cancellationToken);

    private async Task SendAsync(string toEmail, string subject, MimeEntity body, CancellationToken cancellationToken)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = body;

        var secureSocketOptions = _settings.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;

        using var client = new SmtpClient();

        await client.ConnectAsync(_settings.Server, _settings.Port, secureSocketOptions, cancellationToken);
        await client.AuthenticateAsync(_settings.SenderEmail, _settings.Password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
