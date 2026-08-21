namespace Authorization.Application.Accounts.ChangePassword;

public record ChangePasswordResult(
    Guid AccountId,
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);
