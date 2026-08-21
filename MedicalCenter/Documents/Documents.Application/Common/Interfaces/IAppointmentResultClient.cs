using Documents.Application.Common.Dtos;
using ErrorOr;

namespace Documents.Application.Common.Interfaces;

public interface IAppointmentResultClient
{
    Task<ErrorOr<AppointmentResultDocumentDto>> GetAsync(
        Guid appointmentId,
        CancellationToken cancellationToken = default);
}
