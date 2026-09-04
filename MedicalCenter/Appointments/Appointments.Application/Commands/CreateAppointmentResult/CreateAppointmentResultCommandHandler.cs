using Appointments.Application.Common.Interfaces;
using Appointments.Application.Common.Services;
using Appointments.Domain;
using Appointments.Domain.Enums;
using Common.Abstractions.Providers;
using Common.Abstractions.Security;
using ErrorOr;
using MediatR;

namespace Appointments.Application.Commands.CreateAppointmentResult;

public sealed class CreateAppointmentResultCommandHandler
    : IRequestHandler<CreateAppointmentResultCommand, ErrorOr<Guid>>
{
    private readonly IAppointmentQueryRepository _appointmentRepository;
    private readonly IAppointmentResultCommandRepository _resultRepository;
    private readonly IAppointmentResultQueryRepository _resultQueryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly AppointmentResultPublisher _resultPublisher;
    private readonly ClinicClock _clock;
    private readonly TimeProvider _timeProvider;
    private readonly IGuidProvider _guidProvider;

    public CreateAppointmentResultCommandHandler(
        IAppointmentQueryRepository appointmentRepository,
        IAppointmentResultCommandRepository resultRepository,
        IAppointmentResultQueryRepository resultQueryRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserProvider currentUserProvider,
        AppointmentResultPublisher resultPublisher,
        ClinicClock clock,
        TimeProvider timeProvider,
        IGuidProvider guidProvider)
    {
        _appointmentRepository = appointmentRepository;
        _resultRepository = resultRepository;
        _resultQueryRepository = resultQueryRepository;
        _unitOfWork = unitOfWork;
        _currentUserProvider = currentUserProvider;
        _resultPublisher = resultPublisher;
        _clock = clock;
        _timeProvider = timeProvider;
        _guidProvider = guidProvider;
    }

    public async Task<ErrorOr<Guid>> Handle(
        CreateAppointmentResultCommand request,
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

        if (appointment.Status != AppointmentStatus.Approved.ToString())
            return Error.Validation("Appointment.NotApproved", "Appointment results require an approved appointment.");

        if (appointment.Date > _clock.Today
            || (appointment.Date == _clock.Today && appointment.StartTime > _clock.CurrentTime))
        {
            return Error.Validation("Appointment.NotStarted", "Appointment has not started yet.");
        }

        if (await _resultQueryRepository.ExistsForAppointmentAsync(request.AppointmentId, cancellationToken))
            return Error.Conflict("AppointmentResult.AlreadyExists", "Appointment result already exists.");

        var result = AppointmentResult.Create(
            _guidProvider.NewGuid(),
            request.AppointmentId,
            request.Complaints,
            request.Conclusion,
            request.Recommendations,
            request.Diagnosis,
            user.Id!.Value,
            _timeProvider.GetUtcNow());

        await _resultRepository.AddAsync(result, cancellationToken);

        await _resultPublisher.PublishReadyAsync(
            appointment,
            request.Complaints,
            request.Conclusion,
            request.Diagnosis,
            request.Recommendations,
            cancellationToken);

        var outcome = await _unitOfWork.SaveChangesAsync(cancellationToken);

        return outcome == SaveOutcome.Success
            ? result.Id
            : Error.Conflict("AppointmentResult.ConcurrencyConflict", "Result was modified by another operation. Please retry.");
    }
}
