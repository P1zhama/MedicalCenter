using System;

namespace MedicalCenter.Shared.Contracts;

public record WorkerReactivatedEvent(
    Guid AccountId,
    DateTime ReactivatedAt
);
