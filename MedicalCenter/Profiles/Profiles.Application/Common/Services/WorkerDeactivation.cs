using MedicalCenter.Shared.Contracts;
using Profiles.Application.Common.Interfaces;
using Profiles.Domain.Enums;

namespace Profiles.Application.Common.Services;

public sealed class WorkerDeactivation
{
    private readonly IDoctorCommandRepository _doctorRepository;
    private readonly IReceptionistCommandRepository _receptionistRepository;

    public WorkerDeactivation(
        IDoctorCommandRepository doctorCommandRepository,
        IReceptionistCommandRepository receptionistCommandRepository)
    {
        _doctorRepository = doctorCommandRepository;
        _receptionistRepository = receptionistCommandRepository;
    }

    public async Task<IReadOnlyCollection<object>> CascadeByOfficeAsync(
        Guid officeId,
        Guid updatedBy,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        var doctors = await _doctorRepository.GetByOfficeAsync(officeId, cancellationToken);
        var receptionists = await _receptionistRepository.GetByOfficeAsync(officeId, cancellationToken);

        var integrationEvents = new List<object>();

        integrationEvents.AddRange(DeactivateDoctors(doctors, updatedBy, now));
        integrationEvents.AddRange(DeactivateReceptionists(receptionists, updatedBy, now));

        return integrationEvents;
    }

    public async Task<IReadOnlyCollection<object>> CascadeBySpecializationAsync(
        Guid specializationId,
        Guid updatedBy,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        var doctors = await _doctorRepository.GetBySpecializationAsync(specializationId, cancellationToken);

        return DeactivateDoctors(doctors, updatedBy, now).ToList();
    }

    private IEnumerable<object> DeactivateDoctors(
        IReadOnlyList<Domain.Doctor> doctors,
        Guid updatedBy,
        DateTimeOffset now)
    {
        foreach (var doctor in doctors.Where(doctor => doctor.IsActive))
        {
            var expectedVersion = doctor.Version;

            var transition = doctor.ChangeStatus(DoctorStatus.Inactive, updatedBy, now);
            _doctorRepository.Update(doctor, expectedVersion);

            if (transition == StatusTransition.Deactivated)
                yield return new WorkerDeactivatedEvent(doctor.AccountId, now.UtcDateTime);
        }
    }

    private IEnumerable<object> DeactivateReceptionists(
        IReadOnlyList<Domain.Receptionist> receptionists,
        Guid updatedBy,
        DateTimeOffset now)
    {
        foreach (var receptionist in receptionists.Where(receptionist => receptionist.IsActive))
        {
            var expectedVersion = receptionist.Version;

            var transition = receptionist.ChangeStatus(ReceptionistStatus.Inactive, updatedBy, now);
            _receptionistRepository.Update(receptionist, expectedVersion);

            if (transition == StatusTransition.Deactivated)
                yield return new WorkerDeactivatedEvent(receptionist.AccountId, now.UtcDateTime);
        }
    }
}
