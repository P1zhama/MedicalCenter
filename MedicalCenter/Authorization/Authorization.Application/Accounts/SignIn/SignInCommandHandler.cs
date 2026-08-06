using Authorization.Application.Common.Interfaces;
using Authorization.Domain;
using Authorization.Domain.ValueObjects;
using Common.Abstractions.Providers;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Authorization.Application.Accounts.SignIn;

public sealed class SignInCommandHandler
    : IRequestHandler<SignInCommand, ErrorOr<SignInResult>>
{
    private const string EmailNotConfirmedCode = "Account.EmailNotConfirmed";

    private static readonly Error InvalidCredentials = Error.Unauthorized(
        "Account.InvalidCredentials",
        "Either an email or a password is incorrect.");

    private readonly IAccountRepository _accountRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly IGuidProvider _guidProvider;
    private readonly ILogger<SignInCommandHandler> _logger;

    public SignInCommandHandler(
        IAccountRepository accountRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        IGuidProvider guidProvider,
        ILogger<SignInCommandHandler> logger)
    {
        _accountRepository = accountRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
        _guidProvider = guidProvider;
        _logger = logger;
    }

    public async Task<ErrorOr<SignInResult>> Handle(SignInCommand request, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsError)
        {
            _logger.LogWarning("Sign in rejected: email is malformed, {@Errors}", emailResult.Errors);

            return InvalidCredentials;
        }

        var account = await _accountRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
        if (account is null)
        {
            _logger.LogWarning("Sign in failed: no account exists for the requested email");

            return InvalidCredentials;
        }

        var canSignIn = account.EnsureCanSignIn();
        if (canSignIn.IsError)
        {
            _logger.LogWarning(
                "Sign in blocked for account {AccountId}: {@Errors}",
                account.Id,
                canSignIn.Errors);

            return canSignIn.Errors[0].Code == EmailNotConfirmedCode
                ? canSignIn.Errors
                : InvalidCredentials;
        }

        if (!_passwordHasher.Verify(request.Password, account.PasswordHash))
        {
            _logger.LogWarning("Sign in failed for account {AccountId}: wrong password", account.Id);

            return InvalidCredentials;
        }

        var now = _timeProvider.GetUtcNow();

        var accessToken = _jwtTokenGenerator.Generate(account);
        var refreshTokenDescriptor = _refreshTokenGenerator.Generate(now);

        var refreshTokenResult = RefreshToken.Issue(
            _guidProvider.NewGuid(),
            account.Id,
            refreshTokenDescriptor.TokenHash,
            now,
            refreshTokenDescriptor.ExpiresAt);

        if (refreshTokenResult.IsError)
            return refreshTokenResult.Errors;

        await _refreshTokenRepository.AddAsync(refreshTokenResult.Value, cancellationToken);

        if (!await _unitOfWork.TrySaveChangesAsync(cancellationToken))
            return Error.Conflict("Account.ConcurrencyConflict", "Account was modified by another operation. Please retry.");

        _logger.LogInformation(
            "Account {AccountId} signed in, access token expires at {AccessTokenExpiresAt}",
            account.Id,
            accessToken.ExpiresAt);

        return new SignInResult(
            account.Id,
            accessToken.Value,
            accessToken.ExpiresAt,
            refreshTokenDescriptor.Token,
            refreshTokenDescriptor.ExpiresAt);
    }
}
