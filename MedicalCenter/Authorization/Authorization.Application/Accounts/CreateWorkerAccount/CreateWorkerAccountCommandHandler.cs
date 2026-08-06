using Authorization.Application.Common.Interfaces;
using Authorization.Application.Common.Messaging;
using Authorization.Domain;
using Authorization.Domain.Constants;
using Authorization.Domain.ValueObjects;
using Common.Abstractions.Eventing;
using Common.Abstractions.Providers;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Authorization.Application.Accounts.CreateWorkerAccount;

public sealed class CreateWorkerAccountCommandHandler
    : IRequestHandler<CreateWorkerAccountCommand, ErrorOr<Guid>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IPasswordGenerator _passwordGenerator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEventPublisher _eventPublisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly IGuidProvider _guidProvider;
    private readonly ILogger<CreateWorkerAccountCommandHandler> _logger;

    public CreateWorkerAccountCommandHandler(
        IAccountRepository accountRepository,
        IPasswordGenerator passwordGenerator,
        IPasswordHasher passwordHasher,
        IEventPublisher eventPublisher,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        IGuidProvider guidProvider,
        ILogger<CreateWorkerAccountCommandHandler> logger)
    {
        _accountRepository = accountRepository;
        _passwordGenerator = passwordGenerator;
        _passwordHasher = passwordHasher;
        _eventPublisher = eventPublisher;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
        _guidProvider = guidProvider;
        _logger = logger;
    }

    public async Task<ErrorOr<Guid>> Handle(CreateWorkerAccountCommand request, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsError)
            return emailResult.Errors;

        if (!Roles.IsWorker(request.RoleName))
            return Error.Validation("Account.Role", $"Role '{request.RoleName}' is not a worker role.");

        if (await _accountRepository.ExistsByEmailAsync(emailResult.Value, cancellationToken))
            return Error.Conflict("Account.EmailAlreadyUsed", "Someone already uses this email.");

        var password = _passwordGenerator.Generate();
        var passwordHash = _passwordHasher.Hash(password);
        var now = _timeProvider.GetUtcNow();
        var id = _guidProvider.NewGuid();

        var accountResult = Account.CreateWorker(
            id,
            _guidProvider.NewGuid(),
            emailResult.Value,
            passwordHash,
            request.RoleName,
            request.CreatedBy,
            now);
        if (accountResult.IsError)
            return accountResult.Errors;

        await _accountRepository.AddAsync(accountResult.Value, cancellationToken);

        await _eventPublisher.PublishAsync(
            new WorkerCredentialsIssued(id, emailResult.Value.Value, password),
            cancellationToken);

        if (!await _unitOfWork.TrySaveChangesAsync(cancellationToken))
            return Error.Conflict("Account.ConcurrencyConflict", "Account was modified by another operation. Please retry.");

        _logger.LogInformation("Worker account {AccountId} created with role {Role}", id, request.RoleName);

        return id;
    }
}
