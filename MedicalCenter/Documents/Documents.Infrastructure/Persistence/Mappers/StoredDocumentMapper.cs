using Common.Domain;
using Documents.Domain;
using Documents.Domain.Enums;
using Documents.Infrastructure.Persistence.Entities;

namespace Documents.Infrastructure.Persistence.Mappers;

public static class StoredDocumentMapper
{
    public static StoredDocumentEntity ToEntity(this StoredDocument document) => new()
    {
        Id = document.Id,
        ObjectKey = document.ObjectKey,
        FileName = document.FileName,
        ContentType = document.ContentType,
        Size = document.Size,
        Kind = document.Kind.ToString(),
        Visibility = document.Visibility.ToString(),
        OwnerProfileId = document.OwnerProfileId,
        Version = document.Version,
        CreatedBy = document.Audit.CreatedBy,
        CreatedAt = document.Audit.CreatedAt,
        UpdatedBy = document.Audit.UpdatedBy,
        UpdatedAt = document.Audit.UpdatedAt
    };

    public static StoredDocument ToDomain(this StoredDocumentEntity entity)
        => StoredDocument.Restore(
            entity.Id,
            entity.ObjectKey,
            entity.FileName,
            entity.ContentType,
            entity.Size,
            Enum.Parse<DocumentKind>(entity.Kind),
            Enum.Parse<DocumentVisibility>(entity.Visibility),
            entity.OwnerProfileId,
            entity.Version,
            new AuditInfo(entity.CreatedBy, entity.CreatedAt, entity.UpdatedBy, entity.UpdatedAt));
}
