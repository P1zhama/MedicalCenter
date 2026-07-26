namespace Offices.Infrastructure.Persistence;

public class OfficeDocument
{
    public Guid Id { get; set; }

    public string City { get; set; } = null!;

    public string Street { get; set; } = null!;

    public string HouseNumber { get; set; } = null!;

    public string? OfficeNumber { get; set; }

    public string RegistryPhoneNumber { get; set; } = null!;

    public string? PhotoUrl { get; set; }

    public string Status { get; set; } = null!;

    public long Version { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
