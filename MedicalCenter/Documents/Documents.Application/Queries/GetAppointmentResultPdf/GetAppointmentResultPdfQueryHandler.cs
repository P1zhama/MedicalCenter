using Documents.Application.Common.Dtos;
using Documents.Application.Common.Interfaces;
using ErrorOr;
using MediatR;

namespace Documents.Application.Queries.GetAppointmentResultPdf;

public sealed class GetAppointmentResultPdfQueryHandler
    : IRequestHandler<GetAppointmentResultPdfQuery, ErrorOr<GeneratedFileDto>>
{
    private readonly IAppointmentResultClient _client;
    private readonly IPdfRenderer _renderer;

    public GetAppointmentResultPdfQueryHandler(IAppointmentResultClient client, IPdfRenderer renderer)
    {
        _client = client;
        _renderer = renderer;
    }

    public async Task<ErrorOr<GeneratedFileDto>> Handle(
        GetAppointmentResultPdfQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _client.GetAsync(request.AppointmentId, cancellationToken);

        if (result.IsError)
            return result.Errors;

        if (!result.Value.HasResult)
            return Error.NotFound("AppointmentResult.NotFound", "Appointment result was not found.");

        var content = _renderer.RenderAppointmentResult(result.Value);

        return new GeneratedFileDto(
            content,
            "application/pdf",
            $"appointment-result-{result.Value.Date:yyyy-MM-dd}.pdf");
    }
}
