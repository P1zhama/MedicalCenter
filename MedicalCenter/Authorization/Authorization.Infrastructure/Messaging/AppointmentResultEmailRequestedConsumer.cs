using Authorization.Application.Common.Interfaces;
using Authorization.Infrastructure.Notifications;
using MassTransit;
using MedicalCenter.Shared.Contracts;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Messaging;

public sealed class AppointmentResultEmailRequestedConsumer : IConsumer<AppointmentResultEmailRequested>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<AppointmentResultEmailRequestedConsumer> _logger;

    public AppointmentResultEmailRequestedConsumer(
        IAccountRepository accountRepository,
        IEmailSender emailSender,
        ILogger<AppointmentResultEmailRequestedConsumer> logger)
    {
        _accountRepository = accountRepository;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AppointmentResultEmailRequested> context)
    {
        var message = context.Message;

        var account = await _accountRepository.GetByProfileIdAsync(
            message.PatientProfileId,
            context.CancellationToken);

        if (account is null)
        {
            _logger.LogWarning(
                "Profile {ProfileId} has no account, appointment result email is skipped",
                message.PatientProfileId);

            return;
        }

        await _emailSender.SendAppointmentResultAsync(
            account.Email.Value,
            message.FileName,
            message.Content,
            context.CancellationToken);
    }
}
