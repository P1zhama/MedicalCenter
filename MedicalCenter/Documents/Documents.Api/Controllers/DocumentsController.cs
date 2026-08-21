using Documents.Api.ErrorMapping;
using Documents.Api.Models;
using Documents.Application.Commands.DeleteDocument;
using Documents.Application.Commands.UploadDocument;
using Documents.Application.Queries.GetAppointmentResultPdf;
using Documents.Application.Queries.GetDocumentContent;
using Documents.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Documents.Api.Controllers;

[ApiController]
[Route("api/documents")]
public sealed class DocumentsController : ControllerBase
{
    private const long UploadSizeLimitBytes = 6L * 1024 * 1024;
    private const int PublicCacheSeconds = 31536000;

    private readonly ISender _sender;

    public DocumentsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("photos")]
    [RequestSizeLimit(UploadSizeLimitBytes)]
    public async Task<IActionResult> UploadPhoto([FromForm] UploadPhotoRequest request)
    {
        if (request.File is null || request.File.Length == 0)
            return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = "File is required." });

        if (!Enum.TryParse<DocumentKind>(request.Kind, ignoreCase: true, out var kind))
            return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = "Document kind is unknown." });

        using var content = new MemoryStream();
        await request.File.CopyToAsync(content);
        content.Seek(0, SeekOrigin.Begin);

        var result = await _sender.Send(new UploadDocumentCommand(
            kind,
            request.File.FileName,
            request.File.Length,
            content,
            request.OwnerProfileId));

        return result.Match<IActionResult>(
            uploaded => Created(uploaded.Url, uploaded),
            errors => errors.ToProblem());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Download(Guid id)
    {
        var result = await _sender.Send(new GetDocumentContentQuery(id));

        if (result.IsError)
            return result.Errors.ToProblem();

        var document = result.Value;

        Response.Headers.XContentTypeOptions = "nosniff";

        Response.Headers.CacheControl = document.IsPublic
            ? $"public, max-age={PublicCacheSeconds}, immutable"
            : "no-store";

        return File(document.Content, document.ContentType, enableRangeProcessing: true);
    }

    [HttpGet("appointments/{appointmentId:guid}/result.pdf")]
    public async Task<IActionResult> DownloadAppointmentResult(Guid appointmentId)
    {
        var result = await _sender.Send(new GetAppointmentResultPdfQuery(appointmentId));

        if (result.IsError)
            return result.Errors.ToProblem();

        Response.Headers.XContentTypeOptions = "nosniff";
        Response.Headers.CacheControl = "no-store";

        return File(result.Value.Content, result.Value.ContentType, result.Value.FileName);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _sender.Send(new DeleteDocumentCommand(id));

        return result.Match<IActionResult>(
            _ => NoContent(),
            errors => errors.ToProblem());
    }
}
