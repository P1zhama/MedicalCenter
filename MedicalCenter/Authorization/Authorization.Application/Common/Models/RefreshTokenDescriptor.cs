namespace Authorization.Application.Common.Models;

public sealed record RefreshTokenDescriptor(string Token, string TokenHash, DateTimeOffset ExpiresAt);
