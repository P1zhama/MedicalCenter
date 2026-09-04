using Appointments.Application.Common.Interfaces;
using Appointments.Application.Common.Services;
using Common.Abstractions.Security;
using ErrorOr;
using MediatR;

namespace Appointments.Application.Commands.UpdateAppointmentResult;

public sealed class UpdateAppointmentResultCommandHandler
    : IRequestHandler<UpdateAppointmentResultCommand, ErrorOr<Success>>
{
    private readonly IAppointmentQueryRepository _appointmentRepository;
    private readonly IAppointmentResultCommandRepository _resultRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly AppointmentResultPublisher _resultPublisher;
    private readonly TimeProvider _timeProvider;

    public UpdateAppointmentResultCommandHandler(
        IAppointmentQueryRepository appointmentRepository,
        IAppointmentResultCommandRepository resultRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserProvider currentUserProvider,
        AppointmentResultPublisher resultPublisher,
        TimeProvider timeProvider)
    {
        _appointmentRepository = appointmentRepository;
        _resultRepository = resultRepository;
        _unitOfWork = unitOfWork;
        _currentUserProvider = currentUserProvider;
        _resultPublisher = resultPublisher;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<Success>> Handle(
        UpdateAppointmentResultCommand request,
        CancellationToken cancellationToken)
    {
        var user = _currentUserProvider.User;
        if (user?.ProfileId is null)
            return Error.Validation("Doctor.ProfileRequired", "Doctor profile was not found.");

        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken);
        if (appointment is null)
            return Error.NotFound("Appointment.NotFound", "Appointment was not found.");

        if (appointment.DoctorId != user.ProfileId.Value)
            return Error.Forbidden("Appointment.Forbidden", "You are not allowed to perform this action.");

        var result = await _resultRepository.GetByAppointmentIdAsync(request.AppointmentId, cancellationToken);
        if (result is null)
            return Error.NotFound("AppointmentResult.NotFound", "Appointment result was not found.");

        var expectedVersion = result.Version;

        result.Update(
            request.Complaints,
            request.Conclusion,
            request.Recommendations,
            request.Diagnosis,
            user.Id!.Value,
            _timeProvider.GetUtcNow());

        _resultRepository.Update(result, expectedVersion);

        await _resultPublisher.PublishReadyAsync(
            appointment,
            request.Complaints,
            request.Conclusion,
            request.Diagnosis,
            request.Recommendations,
            cancellationToken);

        var outcome = await _unitOfWork.SaveChangesAsync(cancellationToken);

        return outcome == SaveOutcome.Success
            ? Result.Success
            : Error.Conflict("AppointmentResult.ConcurrencyConflict", "Result was modified by another operation. Please retry.");
    }
}
