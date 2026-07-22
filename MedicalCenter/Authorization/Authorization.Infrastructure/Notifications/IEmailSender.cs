namespace Authorization.Infrastructure.Notifications;

public interface IEmailSender
{
    Task SendEmailConfirmationAsync(string email, string token, CancellationToken cancellationToken = default);
}
