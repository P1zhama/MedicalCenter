using Documents.Application.Common.Dtos;
using Documents.Domain.Enums;
using ErrorOr;
using MediatR;

namespace Documents.Application.Commands.UploadDocument;

public record UploadDocumentCommand(
    DocumentKind Kind,
    string FileName,
    long Size,
    Stream Content,
    Guid? OwnerProfileId) : IRequest<ErrorOr<UploadedDocumentDto>>;
