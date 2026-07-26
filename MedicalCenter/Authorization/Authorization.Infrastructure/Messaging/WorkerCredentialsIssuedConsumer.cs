using Authorization.Application.Common.Messaging;
using Authorization.Infrastructure.Notifications;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Messaging;

public sealed class WorkerCredentialsIssuedConsumer : IConsumer<WorkerCredentialsIssued>
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<WorkerCredentialsIssuedConsumer> _logger;

    public WorkerCredentialsIssuedConsumer(
        IEmailSender emailSender,
        ILogger<WorkerCredentialsIssuedConsumer> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<WorkerCredentialsIssued> context)
    {
        var message = context.Message;

        _logger.LogInformation("Sending worker credentials for account {AccountId}", message.AccountId);

        await _emailSender.SendWorkerCredentialsAsync(message.Email, message.TemporaryPassword, context.CancellationToken);
    }
}
