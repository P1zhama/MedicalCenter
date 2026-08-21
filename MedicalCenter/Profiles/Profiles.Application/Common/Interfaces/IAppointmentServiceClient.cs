namespace Profiles.Application.Common.Interfaces;

public interface IAppointmentServiceClient
{
    Task<bool> HasApprovedAppointmentAsync(
        Guid doctorId,
        Guid patientId,
        CancellationToken cancellationToken = default);
}
