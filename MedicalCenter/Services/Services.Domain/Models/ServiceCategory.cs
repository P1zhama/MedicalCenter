using Common.Domain;
using Common.Domain.Exceptions;

namespace Services.Domain.Models;

public sealed class ServiceCategory : Entity<Guid>
{
    private ServiceCategory(Guid id, string name, int timeSlotMinutes) : base(id)
    {
        Name = name;
        TimeSlotMinutes = timeSlotMinutes;
    }

    public string Name { get; private set; }

    public int TimeSlotMinutes { get; private set; }

    public static ServiceCategory Create(Guid id, string name, int timeSlotMinutes)
    {
        var normalizedName = TextNormalization.CollapseWhitespace(Guard.NotNullOrWhiteSpace(name, nameof(name)));
        Guard.MaxLength(normalizedName, 100, nameof(name));

        if (timeSlotMinutes <= 0)
            throw new DomainException("Time slot size must be greater than zero.");

        return new ServiceCategory(id, normalizedName, timeSlotMinutes);
    }

    public static ServiceCategory Restore(Guid id, string name, int timeSlotMinutes)
        => new(id, name, timeSlotMinutes);
}
