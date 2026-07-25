namespace Profiles.Infrastructure.Persistence.Entities;

public class PatientEntity
{
    public Guid Id { get; set; }

    public Guid? AccountId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public string? PhoneNumber { get; set; }

    public DateOnly DateOfBirth { get; set; }

    public string? PhotoUrl { get; set; }

    public long Version { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
