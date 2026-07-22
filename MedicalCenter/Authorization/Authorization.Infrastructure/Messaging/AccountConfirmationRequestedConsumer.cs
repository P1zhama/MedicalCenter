using Authorization.Application.Common.Messaging;
using Authorization.Infrastructure.Notifications;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Messaging;

public sealed class AccountConfirmationRequestedConsumer : IConsumer<AccountConfirmationRequested>
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<AccountConfirmationRequestedConsumer> _logger;

    public AccountConfirmationRequestedConsumer(
        IEmailSender emailSender,
        ILogger<AccountConfirmationRequestedConsumer> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AccountConfirmationRequested> context)
    {
        var message = context.Message;

        _logger.LogInformation("Sending confirmation email for account {AccountId}", message.AccountId);

        await _emailSender.SendEmailConfirmationAsync(message.Email, message.Token, context.CancellationToken);
    }
}
