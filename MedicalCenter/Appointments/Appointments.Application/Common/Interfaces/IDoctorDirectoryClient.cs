using Appointments.Application.Common.Dtos;

namespace Appointments.Application.Common.Interfaces;

public interface IDoctorDirectoryClient
{
    Task<DoctorForAppointmentDto?> GetDoctorAsync(Guid doctorId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> GetAtWorkDoctorIdsAsync(
        Guid specializationId,
        Guid? officeId,
        CancellationToken cancellationToken = default);
}
