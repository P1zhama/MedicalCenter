using Authorization.Application.Accounts.LinkProfile;
using MassTransit;
using MediatR;
using MedicalCenter.Shared.Contracts;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Messaging;

public sealed class ProfileLinkedToAccountEventConsumer : IConsumer<ProfileLinkedToAccountEvent>
{
    private readonly ISender _sender;
    private readonly ILogger<ProfileLinkedToAccountEventConsumer> _logger;

    public ProfileLinkedToAccountEventConsumer(
        ISender sender,
        ILogger<ProfileLinkedToAccountEventConsumer> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProfileLinkedToAccountEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Linking profile {ProfileId} to account {AccountId}",
            message.ProfileId,
            message.AccountId);

        var result = await _sender.Send(
            new LinkProfileCommand(message.AccountId, message.ProfileId),
            context.CancellationToken);

        if (result.IsError)
            throw new InvalidOperationException(
                $"Failed to link profile {message.ProfileId} to account {message.AccountId}: {result.Errors[0].Description}");
    }
}
