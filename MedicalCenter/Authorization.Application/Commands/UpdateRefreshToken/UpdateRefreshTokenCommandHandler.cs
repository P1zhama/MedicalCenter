using Authorization.Application.Common.Dtos;
using Authorization.Application.Common.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Authorization.Application.Commands.RefreshToken;

public class UpdateRefreshTokenCommandHandler : IRequestHandler<UpdateRefreshTokenCommand, AuthResultDto>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public UpdateRefreshTokenCommandHandler(
        IAccountRepository accountRepository, 
        IJwtTokenService jwtTokenService)
    {
        _accountRepository = accountRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResultDto> Handle(UpdateRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (account == null || account.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            // UnauthorizedAccessException, а не голый Exception: gRPC-интерцептор переведёт его
            // в StatusCode.Unauthenticated (клиент получит 401), а не в невнятный Unknown/500
            throw new UnauthorizedAccessException("Invalid or expired refresh token. Please sign in again.");
        }

        var newAccessToken = _jwtTokenService.GenerateAccessToken(account);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

        account.RefreshToken = newRefreshToken;
        account.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await _accountRepository.UpdateAsync(account, cancellationToken);

        return new AuthResultDto(
            AccessToken: newAccessToken,
            RefreshToken: newRefreshToken,
            Message: "Tokens refreshed successfully"
        );
    }
}
