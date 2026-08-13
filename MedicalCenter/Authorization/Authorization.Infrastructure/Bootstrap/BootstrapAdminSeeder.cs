using Authorization.Application.Common.Interfaces;
using Authorization.Domain;
using Authorization.Domain.Constants;
using Authorization.Domain.ValueObjects;
using Common.Abstractions.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Authorization.Infrastructure.Bootstrap;

public sealed class BootstrapAdminSeeder
{
    private readonly IAccountRepository _accountRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly BootstrapAdminSettings _settings;
    private readonly TimeProvider _timeProvider;
    private readonly IGuidProvider _guidProvider;
    private readonly ILogger<BootstrapAdminSeeder> _logger;

    public BootstrapAdminSeeder(
        IAccountRepository accountRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IOptions<BootstrapAdminSettings> settings,
        TimeProvider timeProvider,
        IGuidProvider guidProvider,
        ILogger<BootstrapAdminSeeder> logger)
    {
        _accountRepository = accountRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _settings = settings.Value;
        _timeProvider = timeProvider;
        _guidProvider = guidProvider;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.Email) || string.IsNullOrWhiteSpace(_settings.Password))
        {
            _logger.LogInformation("Bootstrap receptionist is not configured, skipping.");

            return;
        }

        var emailResult = Email.Create(_settings.Email);
        if (emailResult.IsError)
        {
            _logger.LogError("Bootstrap receptionist email is invalid, {@Errors}", emailResult.Errors);

            return;
        }

        if (await _accountRepository.ExistsByEmailAsync(emailResult.Value, cancellationToken))
        {
            _logger.LogInformation("Bootstrap receptionist already exists, skipping.");

            return;
        }

        var now = _timeProvider.GetUtcNow();
        var id = _guidProvider.NewGuid();

        var accountResult = Account.CreateWorker(
            id,
            _guidProvider.NewGuid(),
            emailResult.Value,
            _passwordHasher.Hash(_settings.Password),
            Roles.Receptionist,
            id,
            now);

        if (accountResult.IsError)
        {
            _logger.LogError("Bootstrap receptionist was not created, {@Errors}", accountResult.Errors);

            return;
        }

        await _accountRepository.AddAsync(accountResult.Value, cancellationToken);

        if (!await _unitOfWork.TrySaveChangesAsync(cancellationToken))
        {
            _logger.LogWarning("Bootstrap receptionist was not saved because of a concurrency conflict.");

            return;
        }

        _logger.LogInformation("Bootstrap receptionist {AccountId} created", id);
    }
}
