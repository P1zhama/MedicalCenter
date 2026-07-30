using MedicalCenter.Shared.Contracts;
using Offices.Domain.Models;

namespace Offices.Application.Common.Events;

public static class OfficeIntegrationEvents
{
    public static IReadOnlyCollection<object> ForStatusChange(bool wasActive, Office office, DateTimeOffset now)
    {
        if (!wasActive || office.IsActive)
            return [];

        return [new OfficeDeactivatedEvent(office.Id, now.UtcDateTime)];
    }
}
