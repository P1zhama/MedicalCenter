using System;

namespace MedicalCenter.Shared.Contracts;

public record ProfileLinkedToAccountEvent(
    Guid AccountId,
    Guid PatientId,
    DateTime LinkedAt
);
 