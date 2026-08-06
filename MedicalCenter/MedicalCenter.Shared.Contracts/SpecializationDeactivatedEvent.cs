using System;

namespace MedicalCenter.Shared.Contracts;

public record SpecializationDeactivatedEvent(
    Guid SpecializationId,
    DateTime DeactivatedAt
);
