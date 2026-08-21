using Appointments.Application.Common.Interfaces;
using Appointments.Application.Common.Services;
using Appointments.Domain;
using Common.Abstractions.Security;
using ErrorOr;
using MediatR;

namespace Appointments.Application.Commands.RescheduleAppointment;

public sealed class RescheduleAppointmentCommandHandler
    : IRequestHandler<RescheduleAppointmentCommand, ErrorOr<Success>>,
      IRequestHandler<RescheduleMyAppointmentCommand, ErrorOr<Success>>
{
    private readonly IAppointmentCommandRepository _repository;
    private readonly AppointmentSlotValidator _slotValidator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly AppointmentNotifier _notifier;
    private readonly TimeProvider _timeProvider;

    public RescheduleAppointmentCommandHandler(
        IAppointmentCommandRepository repository,
        AppointmentSlotValidator slotValidator,
        IUnitOfWork unitOfWork,
        ICurrentUserProvider currentUserProvider,
        AppointmentNotifier notifier,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _slotValidator = slotValidator;
        _unitOfWork = unitOfWork;
        _currentUserProvider = currentUserProvider;
        _notifier = notifier;
        _timeProvider = timeProvider;
    }

    public Task<ErrorOr<Success>> Handle(RescheduleAppointmentCommand request, CancellationToken cancellationToken)
        => RescheduleAsync(request.Id, request.DoctorId, request.Date, request.StartTime, null, cancellationToken);

    public Task<ErrorOr<Success>> Handle(RescheduleMyAppointmentCommand request, CancellationToken cancellationToken)
        => RescheduleAsync(
            request.Id,
            request.DoctorId,
            request.Date,
            request.StartTime,
            _currentUserProvider.User?.ProfileId,
            cancellationToken);

    private async Task<ErrorOr<Success>> RescheduleAsync(
        Guid id,
        Guid doctorId,
        DateOnly date,
        TimeOnly startTime,
        Guid? ownerPatientId,
        CancellationToken cancellationToken)
    {
        var user = _currentUserProvider.User;
        if (user is null)
            return Error.Unauthorized("Auth.Unauthenticated", "Authentication is required.");

        var appointment = await _repository.GetByIdAsync(id, cancellationToken);
        if (appointment is null)
            return Error.NotFound("Appointment.NotFound", "Appointment was not found.");

        if (ownerPatientId is not null && appointment.PatientId != ownerPatientId.Value)
            return Error.Forbidden("Appointment.Forbidden", "You are not allowed to perform this action.");

        if (appointment.IsCancelled)
            return Error.Validation("Appointment.Cancelled", "Cancelled appointment cannot be rescheduled.");

        if (appointment.IsApproved)
            return Error.Validation("Appointment.Approved", "Approved appointment cannot be rescheduled.");

        var durationResult = await _slotValidator.ValidateAsync(
            appointment.ServiceId,
            doctorId,
            appointment.OfficeId,
            date,
            startTime,
            excludingAppointmentId: appointment.Id,
            cancellationToken);

        if (durationResult.IsError)
            return durationResult.Errors;

        var expectedVersion = appointment.Version;

        appointment.Reschedule(doctorId, date, startTime, durationResult.Value, user.Id!.Value, _timeProvider.GetUtcNow());

        _repository.Update(appointment, expectedVersion);

        await _notifier.NotifyAsync(appointment, AppointmentNotificationKinds.Rescheduled, cancellationToken);

        var outcome = await _unitOfWork.SaveChangesAsync(cancellationToken);

        return outcome switch
        {
            SaveOutcome.Success => Result.Success,
            SaveOutcome.SlotTaken => Error.Conflict(
                "Appointment.SlotTaken",
                "This time slot is already taken. Please, select another one."),
            _ => Error.Conflict("Appointment.ConcurrencyConflict", "Appointment was modified by another operation. Please retry.")
        };
    }
}
