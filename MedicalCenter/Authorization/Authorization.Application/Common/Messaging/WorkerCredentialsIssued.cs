namespace Authorization.Application.Common.Messaging;

public sealed record WorkerCredentialsIssued(Guid AccountId, string Email, string TemporaryPassword);
