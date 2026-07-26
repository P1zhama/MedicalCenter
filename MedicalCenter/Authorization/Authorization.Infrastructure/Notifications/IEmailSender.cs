namespace Authorization.Infrastructure.Notifications;

public interface IEmailSender
{
    Task SendEmailConfirmationAsync(string email, string token, CancellationToken cancellationToken = default);

    Task SendWorkerCredentialsAsync(string email, string temporaryPassword, CancellationToken cancellationToken = default);
}
