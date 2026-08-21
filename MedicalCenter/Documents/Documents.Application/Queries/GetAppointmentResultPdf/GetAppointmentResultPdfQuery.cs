using Documents.Application.Common.Dtos;
using ErrorOr;
using MediatR;

namespace Documents.Application.Queries.GetAppointmentResultPdf;

public record GetAppointmentResultPdfQuery(Guid AppointmentId) : IRequest<ErrorOr<GeneratedFileDto>>;
