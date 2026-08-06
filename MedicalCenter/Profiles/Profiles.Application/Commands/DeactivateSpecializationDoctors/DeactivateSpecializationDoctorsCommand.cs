using ErrorOr;
using MediatR;

namespace Profiles.Application.Commands.DeactivateSpecializationDoctors;

public sealed record DeactivateSpecializationDoctorsCommand(Guid SpecializationId) : IRequest<ErrorOr<Success>>;
