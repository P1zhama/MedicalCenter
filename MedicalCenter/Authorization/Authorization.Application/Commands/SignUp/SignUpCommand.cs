using MediatR;

namespace Authorization.Application.Commands.SignUp;

public record SignUpCommand
(
    string Email,
    string Password
) : IRequest<Unit>;
