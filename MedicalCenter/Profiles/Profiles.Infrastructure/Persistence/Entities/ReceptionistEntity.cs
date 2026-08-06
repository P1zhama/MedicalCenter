namespace Profiles.Infrastructure.Persistence.Entities;

public class ReceptionistEntity
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public Guid AccountId { get; set; }

    public Guid OfficeId { get; set; }

    public string Status { get; set; } = null!;

    public string? PhotoUrl { get; set; }

    public long Version { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
