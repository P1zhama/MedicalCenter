using ErrorOr;
using MediatR;

namespace Authorization.Application.Accounts.Refresh;

public sealed record RefreshCommand(string RefreshToken)
    : IRequest<ErrorOr<RefreshResult>>;
