using ErrorOr;
using MediatR;

namespace Authorization.Application.Accounts.LinkProfile;

public sealed record LinkProfileCommand(Guid AccountId, Guid ProfileId)
    : IRequest<ErrorOr<Success>>;
