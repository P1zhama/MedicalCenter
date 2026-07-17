using Authorization.Application.Common.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Authorization.Application.Commands.SignOut;

public class SignOutCommandHandler : IRequestHandler<SignOutCommand, bool>
{
    private readonly IAccountRepository _accountRepository;

    public SignOutCommandHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<bool> Handle(SignOutCommand request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (account == null)
        {
            // Токен уже недействителен или отсутствует — считаем, что пользователь и так не в системе,
            // без исключения (раньше здесь искали аккаунт по email, а не по токену, и всегда падали)
            return false;
        }

        account.RefreshToken = null;
        account.RefreshTokenExpiryTime = null;

        await _accountRepository.UpdateAsync(account, cancellationToken);

        return true;
    }
}
