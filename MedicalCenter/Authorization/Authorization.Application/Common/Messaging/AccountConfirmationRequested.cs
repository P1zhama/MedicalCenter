namespace Authorization.Application.Common.Messaging;

public sealed record AccountConfirmationRequested(Guid AccountId, string Email, string Token);
