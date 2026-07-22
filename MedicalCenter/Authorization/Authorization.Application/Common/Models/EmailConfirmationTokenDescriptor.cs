namespace Authorization.Application.Common.Models;

public sealed record EmailConfirmationTokenDescriptor(string Token, string TokenHash, DateTimeOffset ExpiresAt);
