using ErrorOr;
using MediatR;

namespace Authorization.Application.Accounts.SignOut;

public sealed record SignOutCommand(string RefreshToken)
    : IRequest<ErrorOr<Success>>;
