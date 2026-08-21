using Appointments.Domain.Constants;
using Common.Abstractions.Security;
using ErrorOr;
using MediatR;

namespace Appointments.Application.Queries.HasApprovedAppointment;

public record HasApprovedAppointmentQuery(Guid DoctorId, Guid PatientId)
    : IRequest<ErrorOr<bool>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.ViewPatientHistory;
}
