namespace Documents.Application.Common.Dtos;

public record UploadedDocumentDto(
    Guid Id,
    string Url,
    string FileName,
    string ContentType,
    long Size);

public record SweepCandidateDto(Guid Id, string ObjectKey);

public record DocumentContentDto(
    Guid Id,
    Stream Content,
    string ContentType,
    string FileName,
    long Size,
    bool IsPublic);
