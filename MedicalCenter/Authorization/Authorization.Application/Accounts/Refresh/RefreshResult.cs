namespace Authorization.Application.Accounts.Refresh;

public sealed record RefreshResult(
    Guid AccountId,
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);
