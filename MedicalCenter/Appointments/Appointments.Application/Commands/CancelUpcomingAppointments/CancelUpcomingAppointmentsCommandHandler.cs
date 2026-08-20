using Appointments.Application.Common.Interfaces;
using Appointments.Application.Common.Services;
using ErrorOr;
using MediatR;

namespace Appointments.Application.Commands.CancelUpcomingAppointments;

public sealed class CancelUpcomingAppointmentsCommandHandler
    : IRequestHandler<CancelUpcomingByDoctorCommand, ErrorOr<int>>,
      IRequestHandler<CancelUpcomingByServiceCommand, ErrorOr<int>>
{
    private readonly AppointmentCancellation _cancellation;
    private readonly IUnitOfWork _unitOfWork;

    public CancelUpcomingAppointmentsCommandHandler(
        AppointmentCancellation cancellation,
        IUnitOfWork unitOfWork)
    {
        _cancellation = cancellation;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<int>> Handle(CancelUpcomingByDoctorCommand request, CancellationToken cancellationToken)
    {
        var cancelled = await _cancellation.CancelUpcomingByDoctorAsync(request.DoctorId, cancellationToken);

        return await SaveAsync(cancelled, cancellationToken);
    }

    public async Task<ErrorOr<int>> Handle(CancelUpcomingByServiceCommand request, CancellationToken cancellationToken)
    {
        var cancelled = await _cancellation.CancelUpcomingByServiceAsync(request.ServiceId, cancellationToken);

        return await SaveAsync(cancelled, cancellationToken);
    }

    private async Task<ErrorOr<int>> SaveAsync(int cancelled, CancellationToken cancellationToken)
    {
        if (cancelled == 0)
            return 0;

        var outcome = await _unitOfWork.SaveChangesAsync(cancellationToken);

        return outcome == SaveOutcome.Success
            ? cancelled
            : Error.Conflict("Appointment.ConcurrencyConflict", "Appointments were modified by another operation.");
    }
}
