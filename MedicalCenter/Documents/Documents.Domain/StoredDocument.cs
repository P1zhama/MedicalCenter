using Common.Domain;
using Common.Domain.Exceptions;
using Documents.Domain.Enums;

namespace Documents.Domain;

public sealed class StoredDocument : AggregateRoot<Guid>
{
    private const int ObjectKeyMaxLength = 512;
    private const int FileNameMaxLength = 255;
    private const int ContentTypeMaxLength = 128;
    private const long SizeMaxBytes = 5L * 1024 * 1024;

    private StoredDocument(
        Guid id,
        string objectKey,
        string fileName,
        string contentType,
        long size,
        DocumentKind kind,
        DocumentVisibility visibility,
        Guid? ownerProfileId,
        long version,
        AuditInfo audit)
        : base(id, version, audit)
    {
        ObjectKey = objectKey;
        FileName = fileName;
        ContentType = contentType;
        Size = size;
        Kind = kind;
        Visibility = visibility;
        OwnerProfileId = ownerProfileId;
    }

    public string ObjectKey { get; private set; }

    public string FileName { get; private set; }

    public string ContentType { get; private set; }

    public long Size { get; private set; }

    public DocumentKind Kind { get; private set; }

    public DocumentVisibility Visibility { get; private set; }

    public Guid? OwnerProfileId { get; private set; }

    public bool IsPublic => Visibility == DocumentVisibility.Public;

    public static StoredDocument Create(
        Guid id,
        string objectKey,
        string fileName,
        string contentType,
        long size,
        DocumentKind kind,
        Guid? ownerProfileId,
        Guid createdBy,
        DateTimeOffset createdAt)
    {
        Guard.NotEmpty(id, nameof(id));
        Guard.MaxLength(Guard.NotNullOrWhiteSpace(objectKey, nameof(objectKey)), ObjectKeyMaxLength, nameof(objectKey));
        Guard.MaxLength(Guard.NotNullOrWhiteSpace(fileName, nameof(fileName)), FileNameMaxLength, nameof(fileName));
        Guard.MaxLength(Guard.NotNullOrWhiteSpace(contentType, nameof(contentType)), ContentTypeMaxLength, nameof(contentType));

        if (size <= 0)
            throw new DomainException("Document size must be greater than zero.");

        if (size > SizeMaxBytes)
            throw new DomainException($"Document size must not exceed {SizeMaxBytes} bytes.");

        if (!Enum.IsDefined(kind))
            throw new DomainException("Document kind is unknown.");

        if (ownerProfileId == Guid.Empty)
            throw new DomainException("Document owner profile id must not be empty.");

        return new StoredDocument(
            id,
            objectKey,
            fileName,
            contentType,
            size,
            kind,
            VisibilityFor(kind),
            ownerProfileId,
            version: 1,
            new AuditInfo(createdBy, createdAt, null, null));
    }

    public static StoredDocument Restore(
        Guid id,
        string objectKey,
        string fileName,
        string contentType,
        long size,
        DocumentKind kind,
        DocumentVisibility visibility,
        Guid? ownerProfileId,
        long version,
        AuditInfo audit)
        => new(id, objectKey, fileName, contentType, size, kind, visibility, ownerProfileId, version, audit);

    private static DocumentVisibility VisibilityFor(DocumentKind kind)
        => kind == DocumentKind.DoctorPhoto
            ? DocumentVisibility.Public
            : DocumentVisibility.Private;
}
