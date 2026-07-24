using System;
using System.ComponentModel.DataAnnotations;

namespace Gateway.API.Models;

public record SignUpWebRequest(
    [Required][EmailAddress] string Email,
    [Required][MinLength(6)][MaxLength(15)] string Password
);

public record SignUpWebResponse(string AccountId);

public record SignInWebRequest(
    [Required][EmailAddress] string Email,
    [Required][MinLength(6)][MaxLength(15)] string Password
);

public record SignInWebResponse(
    string AccountId,
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt
);

public record ConfirmEmailWebRequest(
    [Required] string Token
);

public record ConfirmEmailWebResponse(
    string AccountId
);

public record RefreshWebRequest(
    [Required] string RefreshToken
);

public record RefreshWebResponse(
    string AccountId,
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt
);

public record SignOutWebRequest(
    [Required] string RefreshToken
);
