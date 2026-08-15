using ErrorOr;
using MediatR;

namespace Profiles.Application.Queries.GetDoctorsForAppointment;

public record GetDoctorsForAppointmentQuery(Guid SpecializationId, Guid? OfficeId)
    : IRequest<ErrorOr<IReadOnlyList<Guid>>>;
