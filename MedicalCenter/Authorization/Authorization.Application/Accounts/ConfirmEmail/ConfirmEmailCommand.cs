using ErrorOr;
using MediatR;

namespace Authorization.Application.Accounts.ConfirmEmail;

public sealed record ConfirmEmailCommand(string Token)
    : IRequest<ErrorOr<Guid>>;
