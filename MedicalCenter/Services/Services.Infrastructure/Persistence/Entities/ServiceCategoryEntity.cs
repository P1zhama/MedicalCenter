namespace Services.Infrastructure.Persistence.Entities;

public class ServiceCategoryEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public int TimeSlotMinutes { get; set; }
}
