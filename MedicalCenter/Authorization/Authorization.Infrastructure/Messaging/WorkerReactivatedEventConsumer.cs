using Authorization.Application.Accounts.SetAccountActivation;
using MassTransit;
using MediatR;
using MedicalCenter.Shared.Contracts;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Messaging;

public sealed class WorkerReactivatedEventConsumer : IConsumer<WorkerReactivatedEvent>
{
    private readonly ISender _sender;
    private readonly ILogger<WorkerReactivatedEventConsumer> _logger;

    public WorkerReactivatedEventConsumer(
        ISender sender,
        ILogger<WorkerReactivatedEventConsumer> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<WorkerReactivatedEvent> context)
    {
        var accountId = context.Message.AccountId;

        _logger.LogInformation("Worker profile reactivated — reactivating account {AccountId}", accountId);

        var result = await _sender.Send(
            new SetAccountActivationCommand(accountId, IsActive: true),
            context.CancellationToken);

        if (result.IsError)
            throw new InvalidOperationException(
                $"Failed to reactivate account {accountId}: {result.Errors[0].Description}");
    }
}
