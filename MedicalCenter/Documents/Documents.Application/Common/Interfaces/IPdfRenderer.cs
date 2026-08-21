using Documents.Application.Common.Dtos;

namespace Documents.Application.Common.Interfaces;

public interface IPdfRenderer
{
    byte[] RenderAppointmentResult(AppointmentResultDocumentDto result);
}
