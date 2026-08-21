using Authorization.Application.Common.Interfaces;
using Authorization.Infrastructure.Notifications;
using MassTransit;
using MedicalCenter.Shared.Contracts;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Messaging;

public sealed class AppointmentNotificationRequestedConsumer : IConsumer<AppointmentNotificationRequested>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<AppointmentNotificationRequestedConsumer> _logger;

    public AppointmentNotificationRequestedConsumer(
        IAccountRepository accountRepository,
        IEmailSender emailSender,
        ILogger<AppointmentNotificationRequestedConsumer> logger)
    {
        _accountRepository = accountRepository;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AppointmentNotificationRequested> context)
    {
        var message = context.Message;

        var account = await _accountRepository.GetByProfileIdAsync(
            message.PatientProfileId,
            context.CancellationToken);

        if (account is null)
        {
            _logger.LogInformation(
                "Profile {ProfileId} has no account, appointment {Kind} notification is skipped",
                message.PatientProfileId,
                message.Kind);

            return;
        }

        await _emailSender.SendAppointmentNotificationAsync(
            account.Email.Value,
            new AppointmentNotification(
                message.Kind,
                message.AppointmentStart,
                message.DoctorFullName,
                message.ServiceName),
            context.CancellationToken);
    }
}
