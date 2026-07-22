using ErrorOr;
using MediatR;

namespace Authorization.Application.Accounts.SignIn;

public sealed record SignInCommand(string Email, string Password)
    : IRequest<ErrorOr<SignInResult>>;
