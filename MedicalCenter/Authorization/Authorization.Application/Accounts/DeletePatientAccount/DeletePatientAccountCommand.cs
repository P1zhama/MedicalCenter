using ErrorOr;
using MediatR;

namespace Authorization.Application.Accounts.DeletePatientAccount;

public sealed record DeletePatientAccountCommand(Guid AccountId) : IRequest<ErrorOr<Success>>;
