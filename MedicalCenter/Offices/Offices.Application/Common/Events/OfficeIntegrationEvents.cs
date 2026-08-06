using MedicalCenter.Shared.Contracts;
using Offices.Domain.Models;

namespace Offices.Application.Common.Events;

public static class OfficeIntegrationEvents
{
    public static IReadOnlyCollection<object> ForDeactivation(bool deactivated, Office office, DateTimeOffset now)
    {
        if (!deactivated)
            return [];

        return [new OfficeDeactivatedEvent(office.Id, now.UtcDateTime)];
    }
}
