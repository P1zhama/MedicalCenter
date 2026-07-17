using Authorization.Application.Common.Dtos;
using MediatR;

namespace Authorization.Application.Commands.ConfirmEmail;

public record ConfirmEmailCommand(string Token) : IRequest<AuthResultDto>;