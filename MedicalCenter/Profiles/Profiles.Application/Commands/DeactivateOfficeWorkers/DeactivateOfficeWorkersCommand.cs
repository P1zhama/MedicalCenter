using ErrorOr;
using MediatR;

namespace Profiles.Application.Commands.DeactivateOfficeWorkers;

public sealed record DeactivateOfficeWorkersCommand(Guid OfficeId) : IRequest<ErrorOr<Success>>;
