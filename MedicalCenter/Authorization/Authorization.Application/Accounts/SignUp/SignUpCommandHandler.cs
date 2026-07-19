using Authorization.Application.Common.Interfaces;
using Authorization.Domain;
using Authorization.Domain.ValueObjects;
using Common.Abstractions.Providers;
using ErrorOr;
using MediatR;

namespace Authorization.Application.Accounts.SignUp;

public sealed class SignUpCommandHandler
    : IRequestHandler<SignUpCommand, ErrorOr<Guid>>
{
    private const int PasswordMinLength = 6;
    private const int PasswordMaxLength = 15;

    private readonly IAccountRepository _accountRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly IGuidProvider _guidProvider;

    public SignUpCommandHandler(
        IAccountRepository accountRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        IGuidProvider guidProvider)
    {
        _accountRepository = accountRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
        _guidProvider = guidProvider;
    }

    public async Task<ErrorOr<Guid>> Handle(SignUpCommand request, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsError)
            return emailResult.Errors;

        if (request.Password.Length < PasswordMinLength || request.Password.Length > PasswordMaxLength)
            return Error.Validation("Password.Invalid",
                $"Password must be between {PasswordMinLength} and {PasswordMaxLength} characters.");

        if (await _accountRepository.ExistsByEmailAsync(emailResult.Value, cancellationToken))
            return Error.Conflict("Account.EmailAlreadyUsed", "Someone already uses this email.");

        var passwordHash = _passwordHasher.Hash(request.Password);
        var now = _timeProvider.GetUtcNow();
        var id = _guidProvider.NewGuid();

        var accountResult = Account.CreateNew(id, request.Email, passwordHash, createdBy: id, now);
        if (accountResult.IsError)
            return accountResult.Errors;

        await _accountRepository.AddAsync(accountResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return accountResult.Value.Id;
    }
}
