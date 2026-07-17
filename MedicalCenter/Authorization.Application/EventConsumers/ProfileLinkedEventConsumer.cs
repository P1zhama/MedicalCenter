
using Authorization.Application.Common.Interfaces;
using MassTransit;
using MedicalCenter.Shared.Contracts;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Authorization.Application.EventConsumers;

public class ProfileLinkedEventConsumer : IConsumer<ProfileLinkedToAccountEvent>
{
    private readonly ILogger<ProfileLinkedEventConsumer> _logger;
    private readonly IAccountRepository _accountRepository;

    public ProfileLinkedEventConsumer(
        ILogger<ProfileLinkedEventConsumer> logger,
        IAccountRepository accountRepository)
    {
        _logger = logger;
        _accountRepository = accountRepository;
    }

    public async Task Consume(ConsumeContext<ProfileLinkedToAccountEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation("Processing ProfileLinkedToAccountEvent for AccountId: {AccountId}", message.AccountId);

        var account = await _accountRepository.GetByIdAsync(message.AccountId, context.CancellationToken);

        if (account == null)
        {
            _logger.LogWarning("Account with ID {AccountId} not found. Cannot link profile.", message.AccountId);
            return;
        }

        if (account.IsProfileCreated)
        {
            // Событие могло прийти повторно (ретраи брокера) — повторно ничего не пишем
            _logger.LogInformation("Account {AccountId} is already marked as profile-created. Skipping.", message.AccountId);
            return;
        }

        account.IsProfileCreated = true;
        await _accountRepository.UpdateAsync(account, context.CancellationToken);

        _logger.LogInformation("Account {AccountId} marked as profile-created.", message.AccountId);
    }
}
