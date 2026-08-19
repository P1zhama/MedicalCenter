using Appointments.Application.Common.Dtos;

namespace Appointments.Application.Common.Interfaces;

public interface IPatientDirectoryClient
{
    Task<bool> ExistsAsync(Guid patientId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PatientSummaryDto>> GetSummariesAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);
}
