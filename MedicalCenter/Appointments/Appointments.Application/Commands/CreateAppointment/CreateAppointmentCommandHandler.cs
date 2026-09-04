using Appointments.Application.Common.Interfaces;
using Appointments.Application.Common.Services;
using Appointments.Domain;
using Common.Abstractions.Providers;
using Common.Abstractions.Security;
using ErrorOr;
using MediatR;

namespace Appointments.Application.Commands.CreateAppointment;

public sealed class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, ErrorOr<Guid>>
{
    private readonly IAppointmentCommandRepository _repository;
    private readonly AppointmentSlotValidator _slotValidator;
    private readonly IPatientDirectoryClient _patientDirectoryClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly TimeProvider _timeProvider;
    private readonly IGuidProvider _guidProvider;

    public CreateAppointmentCommandHandler(
        IAppointmentCommandRepository repository,
        AppointmentSlotValidator slotValidator,
        IPatientDirectoryClient patientDirectoryClient,
        IUnitOfWork unitOfWork,
        ICurrentUserProvider currentUserProvider,
        TimeProvider timeProvider,
        IGuidProvider guidProvider)
    {
        _repository = repository;
        _slotValidator = slotValidator;
        _patientDirectoryClient = patientDirectoryClient;
        _unitOfWork = unitOfWork;
        _currentUserProvider = currentUserProvider;
        _timeProvider = timeProvider;
        _guidProvider = guidProvider;
    }

    public async Task<ErrorOr<Guid>> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var user = _currentUserProvider.User;
        if (user is null)
            return Error.Unauthorized("Auth.Unauthenticated", "Authentication is required.");

        if (!user.HasProfile)
            return Error.Validation("Patient.ProfileRequired", "Please, create your profile first.");

        var durationResult = await _slotValidator.ValidateAsync(
            request.ServiceId,
            request.DoctorId,
            request.OfficeId,
            request.Date,
            request.StartTime,
            excludingAppointmentId: null,
            cancellationToken);

        if (durationResult.IsError)
            return durationResult.Errors;

        var patient = (await _patientDirectoryClient.GetSummariesAsync([user.ProfileId!.Value], cancellationToken))
            .FirstOrDefault();

        var appointment = Appointment.Create(
            _guidProvider.NewGuid(),
            user.ProfileId!.Value,
            PatientNames.Full(patient),
            request.DoctorId,
            request.ServiceId,
            request.OfficeId,
            request.Date,
            request.StartTime,
            durationResult.Value,
            user.Id!.Value,
            _timeProvider.GetUtcNow());

        await _repository.AddAsync(appointment, cancellationToken);

        var outcome = await _unitOfWork.SaveChangesAsync(cancellationToken);

        return outcome switch
        {
            SaveOutcome.Success => appointment.Id,
            SaveOutcome.SlotTaken => Error.Conflict(
                "Appointment.SlotTaken",
                "This time slot is already taken. Please, select another one."),
            _ => Error.Conflict("Appointment.ConcurrencyConflict", "Appointment was modified by another operation. Please retry.")
        };
    }
}
