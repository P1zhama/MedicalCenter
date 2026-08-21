namespace Authorization.Infrastructure.Notifications;

public interface IEmailSender
{
    Task SendEmailConfirmationAsync(string email, string token, CancellationToken cancellationToken = default);

    Task SendWorkerCredentialsAsync(string email, string temporaryPassword, CancellationToken cancellationToken = default);

    Task SendAppointmentNotificationAsync(
        string email,
        AppointmentNotification notification,
        CancellationToken cancellationToken = default);

    Task SendAppointmentResultAsync(
        string email,
        string fileName,
        byte[] content,
        CancellationToken cancellationToken = default);
}
