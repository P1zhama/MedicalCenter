namespace Authorization.Application.Accounts.SignIn;

public sealed record SignInResult(
    Guid AccountId,
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);
