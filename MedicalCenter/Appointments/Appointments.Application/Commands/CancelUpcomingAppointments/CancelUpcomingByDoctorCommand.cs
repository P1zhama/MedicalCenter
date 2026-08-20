using ErrorOr;
using MediatR;

namespace Appointments.Application.Commands.CancelUpcomingAppointments;

public record CancelUpcomingByDoctorCommand(Guid DoctorId) : IRequest<ErrorOr<int>>;

public record CancelUpcomingByServiceCommand(Guid ServiceId) : IRequest<ErrorOr<int>>;
