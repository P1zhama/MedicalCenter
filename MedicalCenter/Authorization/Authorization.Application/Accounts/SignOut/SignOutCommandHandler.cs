using Authorization.Application.Common.Interfaces;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Authorization.Application.Accounts.SignOut;

public sealed class SignOutCommandHandler
    : IRequestHandler<SignOutCommand, ErrorOr<Success>>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenHashGenerator _tokenHashGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<SignOutCommandHandler> _logger;

    public SignOutCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        ITokenHashGenerator tokenHashGenerator,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        ILogger<SignOutCommandHandler> logger)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _tokenHashGenerator = tokenHashGenerator;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<ErrorOr<Success>> Handle(SignOutCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _tokenHashGenerator.Hash(request.RefreshToken);

        var storedToken = await _refreshTokenRepository.GetByHashAsync(tokenHash, cancellationToken);
        var now = _timeProvider.GetUtcNow();

        if (storedToken is null || !storedToken.IsActive(now))
        {
            _logger.LogInformation("Sign out called with a missing or already inactive refresh token; treated as success");

            return Result.Success;
        }

        var revoke = storedToken.Revoke(now);
        if (revoke.IsError)
            return revoke.Errors;

        await _refreshTokenRepository.UpdateAsync(storedToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Account {AccountId} signed out; refresh token revoked", storedToken.AccountId);

        return Result.Success;
    }
}
