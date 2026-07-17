using MediatR;


namespace Authorization.Application.Commands.SignOut;

public record SignOutCommand(string RefreshToken) : IRequest<bool>;