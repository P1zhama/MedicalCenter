using Authorization.Application.Common.Interfaces;
using Authorization.Application.Common.Messaging;
using Authorization.Domain;
using Authorization.Domain.Constants;
using Authorization.Domain.ValueObjects;
using Common.Abstractions.Providers;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Authorization.Application.Accounts.SignUp;

public sealed class SignUpCommandHandler
    : IRequestHandler<SignUpCommand, ErrorOr<Guid>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailConfirmationTokenGenerator _emailConfirmationTokenGenerator;
    private readonly IEventPublisher _eventPublisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly IGuidProvider _guidProvider;
    private readonly ILogger<SignUpCommandHandler> _logger;

    public SignUpCommandHandler(
        IAccountRepository accountRepository,
        IPasswordHasher passwordHasher,
        IEmailConfirmationTokenGenerator emailConfirmationTokenGenerator,
        IEventPublisher eventPublisher,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        IGuidProvider guidProvider,
        ILogger<SignUpCommandHandler> logger)
    {
        _accountRepository = accountRepository;
        _passwordHasher = passwordHasher;
        _emailConfirmationTokenGenerator = emailConfirmationTokenGenerator;
        _eventPublisher = eventPublisher;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
        _guidProvider = guidProvider;
        _logger = logger;
    }

    public async Task<ErrorOr<Guid>> Handle(SignUpCommand request, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsError)
            return emailResult.Errors;

        var existing = await _accountRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
        if (existing is not null && existing.IsEmailConfirmed)
            return Error.Conflict("Account.EmailAlreadyUsed", "Someone already uses this email.");

        var passwordHash = _passwordHasher.Hash(request.Password);
        var now = _timeProvider.GetUtcNow();
        var confirmationToken = _emailConfirmationTokenGenerator.Generate(now);

        if (existing is not null)
        {
            var reissue = existing.ReissueConfirmation(
                passwordHash,
                confirmationToken.TokenHash,
                confirmationToken.ExpiresAt,
                now);

            if (reissue.IsError)
                return reissue.Errors;

            await _accountRepository.UpdateAsync(existing, cancellationToken);

            await _eventPublisher.PublishAsync(
                new AccountConfirmationRequested(existing.Id, existing.Email.Value, confirmationToken.Token),
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Reissued email confirmation for pending account {AccountId}", existing.Id);

            return existing.Id;
        }

        var id = _guidProvider.NewGuid();

        var accountResult = Account.CreateNew(
            id,
            emailResult.Value,
            passwordHash,
            Roles.Patient,
            confirmationToken.TokenHash,
            confirmationToken.ExpiresAt,
            createdBy: id,
            now);

        if (accountResult.IsError)
            return accountResult.Errors;

        await _accountRepository.AddAsync(accountResult.Value, cancellationToken);

        await _eventPublisher.PublishAsync(
            new AccountConfirmationRequested(accountResult.Value.Id, accountResult.Value.Email.Value, confirmationToken.Token),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Account {AccountId} signed up with role {Role}",
            accountResult.Value.Id,
            Roles.Patient);

        return accountResult.Value.Id;
    }
}
