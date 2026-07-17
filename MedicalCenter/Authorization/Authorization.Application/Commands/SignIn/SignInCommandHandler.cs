using Authorization.Application.Common.Dtos;
using Authorization.Application.Common.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Authorization.Application.Commands.SignIn;

public class SignInCommandHandler : IRequestHandler<SignInCommand, AuthResultDto>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public SignInCommandHandler(IAccountRepository accountRepository, IJwtTokenService jwtTokenService)
    {
        _jwtTokenService = jwtTokenService;
        _accountRepository = accountRepository;
    }

    public async Task<AuthResultDto> Handle(SignInCommand request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (account == null || !BCrypt.Net.BCrypt.Verify(request.Password, account.Password))
        {
            throw new UnauthorizedAccessException("Either an email or a password is incorrect");
        }

        if (!account.IsEmailVerified)
        {
            throw new UnauthorizedAccessException("Please confirm your email before signing in");
        }

        var accessToken = _jwtTokenService.GenerateAccessToken(account);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        account.RefreshToken = refreshToken;
        account.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30);

        await _accountRepository.UpdateAsync(account, cancellationToken);
                
        return new AuthResultDto 
        (
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            Message: "You've signed in successfully"
        );
    }
}