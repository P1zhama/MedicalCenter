namespace Appointments.Infrastructure.Persistence.Entities;

public class AppointmentResultEntity
{
    public Guid Id { get; set; }

    public Guid AppointmentId { get; set; }

    public string Complaints { get; set; } = null!;

    public string Conclusion { get; set; } = null!;

    public string Recommendations { get; set; } = null!;

    public string? Diagnosis { get; set; }

    public long Version { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
