using ErrorOr;
using MediatR;

namespace Documents.Application.Commands.SendAppointmentResult;

public record SendAppointmentResultCommand(Guid AppointmentId) : IRequest<ErrorOr<Success>>;
