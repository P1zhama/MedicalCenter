namespace Appointments.Application.Common.Interfaces;

public interface IPatientDirectoryClient
{
    Task<bool> ExistsAsync(Guid patientId, CancellationToken cancellationToken = default);
}
