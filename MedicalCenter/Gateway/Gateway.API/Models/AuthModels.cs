using System;

namespace Gateway.Api.Models;

public record SignUpWebRequest(
    string Email,
    string Password
);

public record SignUpWebResponse(string AccountId);

public record SignInWebRequest(
    string Email,
    string Password
);

public record SignInWebResponse(
    string AccountId,
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt
);

public record ConfirmEmailWebRequest(
    string Token
);

public record ConfirmEmailWebResponse(
    string AccountId
);

public record RefreshWebRequest(
    string RefreshToken
);

public record RefreshWebResponse(
    string AccountId,
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt
);

public record SignOutWebRequest(
    string RefreshToken
);
