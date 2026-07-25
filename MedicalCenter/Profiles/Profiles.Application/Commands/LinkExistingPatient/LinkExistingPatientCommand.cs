using ErrorOr;
using MediatR;

namespace Profiles.Application.Commands.LinkExistingPatient;

public record LinkExistingPatientCommand(
    Guid AccountId,
    Guid PatientId
) : IRequest<ErrorOr<Success>>;
