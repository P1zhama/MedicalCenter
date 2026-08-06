using MedicalCenter.Shared.Contracts;
using Profiles.Domain.Enums;

namespace Profiles.Application.Common.Services;

public static class WorkerStatusEvents
{
    public static object? ForTransition(StatusTransition transition, Guid accountId, DateTimeOffset now)
        => transition switch
        {
            StatusTransition.Deactivated => new WorkerDeactivatedEvent(accountId, now.UtcDateTime),
            StatusTransition.Reactivated => new WorkerReactivatedEvent(accountId, now.UtcDateTime),
            _ => null
        };
}
