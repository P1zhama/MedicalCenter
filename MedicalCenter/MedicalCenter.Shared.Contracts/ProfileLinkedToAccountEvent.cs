using System;

namespace MedicalCenter.Shared.Contracts;

public record ProfileLinkedToAccountEvent(
    Guid AccountId,
    Guid ProfileId,
    DateTime LinkedAt
);
 