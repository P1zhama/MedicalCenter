using Authorization.Application.Common.Dtos;
using MediatR;

namespace Authorization.Application.Commands.SignIn;

public record SignInCommand(string Email, string Password) : IRequest<AuthResultDto>;