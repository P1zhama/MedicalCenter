using Appointments.Application.Common.Interfaces;
using Appointments.Application.Common.Services;
using Common.Abstractions.Security;
using ErrorOr;
using MediatR;

namespace Appointments.Application.Commands.CancelAppointment;

public sealed class CancelAppointmentCommandHandler : IRequestHandler<CancelAppointmentCommand, ErrorOr<Success>>
{
    private readonly IAppointmentCommandRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly AppointmentNotifier _notifier;
    private readonly TimeProvider _timeProvider;

    public CancelAppointmentCommandHandler(
        IAppointmentCommandRepository repository,
        IUnitOfWork unitOfWork,
        ICurrentUserProvider currentUserProvider,
        AppointmentNotifier notifier,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserProvider = currentUserProvider;
        _notifier = notifier;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<Success>> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        var updatedBy = _currentUserProvider.User?.Id;
        if (updatedBy is null)
            return Error.Unauthorized("Auth.Unauthenticated", "Authentication is required.");

        var appointment = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (appointment is null)
            return Error.NotFound("Appointment.NotFound", "Appointment was not found.");

        if (appointment.IsCancelled)
            return Error.Conflict("Appointment.AlreadyCancelled", "Appointment is already cancelled.");

        var expectedVersion = appointment.Version;

        appointment.Cancel(updatedBy.Value, _timeProvider.GetUtcNow());

        _repository.Update(appointment, expectedVersion);

        await _notifier.NotifyAsync(appointment, AppointmentNotificationKinds.Cancelled, cancellationToken);

        var outcome = await _unitOfWork.SaveChangesAsync(cancellationToken);

        return outcome == SaveOutcome.Success
            ? Result.Success
            : Error.Conflict("Appointment.ConcurrencyConflict", "Appointment was modified by another operation. Please retry.");
    }
}
