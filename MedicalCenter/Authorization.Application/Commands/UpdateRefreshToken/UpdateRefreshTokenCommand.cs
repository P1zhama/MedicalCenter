using Authorization.Application.Common.Dtos;
using MediatR;

namespace Authorization.Application.Commands.RefreshToken;

public record UpdateRefreshTokenCommand(string RefreshToken) : IRequest<AuthResultDto>;
