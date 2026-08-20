using System;

namespace MedicalCenter.Shared.Contracts;

public record WorkerDeactivatedEvent(
    Guid AccountId,
    Guid ProfileId,
    DateTime DeactivatedAt
);
