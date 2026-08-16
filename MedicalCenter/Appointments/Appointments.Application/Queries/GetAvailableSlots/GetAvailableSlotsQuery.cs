using Appointments.Application.Common.Dtos;
using ErrorOr;
using MediatR;

namespace Appointments.Application.Queries.GetAvailableSlots;

public record GetAvailableSlotsQuery(
    DateOnly Date,
    Guid ServiceId,
    Guid? DoctorId,
    Guid? OfficeId
) : IRequest<ErrorOr<IReadOnlyList<AvailableSlotDto>>>;
