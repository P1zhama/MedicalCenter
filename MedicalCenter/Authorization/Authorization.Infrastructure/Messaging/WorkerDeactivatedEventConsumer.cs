using Authorization.Application.Accounts.SetAccountActivation;
using MassTransit;
using MediatR;
using MedicalCenter.Shared.Contracts;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Messaging;

public sealed class WorkerDeactivatedEventConsumer : IConsumer<WorkerDeactivatedEvent>
{
    private readonly ISender _sender;
    private readonly ILogger<WorkerDeactivatedEventConsumer> _logger;

    public WorkerDeactivatedEventConsumer(
        ISender sender,
        ILogger<WorkerDeactivatedEventConsumer> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<WorkerDeactivatedEvent> context)
    {
        var accountId = context.Message.AccountId;

        _logger.LogInformation("Worker profile deactivated — deactivating account {AccountId}", accountId);

        var result = await _sender.Send(
            new SetAccountActivationCommand(accountId, IsActive: false),
            context.CancellationToken);

        if (result.IsError)
            throw new InvalidOperationException(
                $"Failed to deactivate account {accountId}: {result.Errors[0].Description}");
    }
}
