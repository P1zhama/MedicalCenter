using System;

namespace MedicalCenter.Shared.Contracts;

public record ServiceDeactivatedEvent(
    Guid ServiceId,
    Guid SpecializationId,
    DateTime DeactivatedAt
);
