using System;

namespace MedicalCenter.Shared.Contracts;

public record OfficeDeactivatedEvent(
    Guid OfficeId,
    DateTime DeactivatedAt
);
