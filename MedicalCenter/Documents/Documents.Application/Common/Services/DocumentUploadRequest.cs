using Documents.Domain.Enums;

namespace Documents.Application.Common.Services;

public sealed record DocumentUploadRequest(
    Stream Content,
    string FileName,
    long Size,
    DocumentKind Kind,
    Guid? OwnerProfileId,
    Guid UploadedBy);
