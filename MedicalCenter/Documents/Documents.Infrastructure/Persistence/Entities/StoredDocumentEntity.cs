namespace Documents.Infrastructure.Persistence.Entities;

public class StoredDocumentEntity
{
    public Guid Id { get; set; }

    public string ObjectKey { get; set; } = null!;

    public string FileName { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public long Size { get; set; }

    public string Kind { get; set; } = null!;

    public string Visibility { get; set; } = null!;

    public Guid? OwnerProfileId { get; set; }

    public DateTimeOffset? LastVerifiedAt { get; set; }

    public long Version { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
