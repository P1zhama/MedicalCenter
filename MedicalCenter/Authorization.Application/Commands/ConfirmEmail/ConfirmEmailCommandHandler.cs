using Authorization.Application.Common.Dtos;
using Authorization.Application.Common.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Authorization.Application.Commands.ConfirmEmail;

public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, AuthResultDto>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public ConfirmEmailCommandHandler(IAccountRepository accountRepository, IJwtTokenService jwtTokenService)
    {
        _accountRepository = accountRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResultDto> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var emailFromToken = _jwtTokenService.ValidateEmailConfirmationToken(request.Token);

        if (string.IsNullOrEmpty(emailFromToken))
        {
            throw new InvalidOperationException("Invalid or expired confirmation token.");
        }

        var account = await _accountRepository.GetByEmailAsync(emailFromToken, cancellationToken);
        if (account == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        account.IsEmailVerified = true;

        // Сразу выдаём токены: по ТЗ (US-8, AC-1) после перехода по ссылке из письма
        // пользователь должен попасть на страницу создания профиля, а она требует
        // авторизованный запрос — без этого пришлось бы просить пользователя войти заново
        var accessToken = _jwtTokenService.GenerateAccessToken(account);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        account.RefreshToken = refreshToken;
        account.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30);

        await _accountRepository.UpdateAsync(account, cancellationToken);

        return new AuthResultDto(accessToken, refreshToken, "Email confirmed successfully");
    }
}
