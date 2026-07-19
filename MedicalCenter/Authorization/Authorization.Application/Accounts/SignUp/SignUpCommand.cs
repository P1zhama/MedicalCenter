using ErrorOr;
using MediatR;

namespace Authorization.Application.Accounts.SignUp;

public sealed record SignUpCommand(string Email, string Password)
    : IRequest<ErrorOr<Guid>>;
