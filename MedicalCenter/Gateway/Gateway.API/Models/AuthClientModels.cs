using System.ComponentModel.DataAnnotations;

namespace Gateway.API.Models;

public record SignUpWebRequest(
    [Required][EmailAddress] string Email,
    [Required][MinLength(6)] string Password
);

public record SignInWebRequest(
    [Required][EmailAddress] string Email,
    [Required] string Password
);

public record SignOutWebRequest(
    [Required] string RefreshToken
);

public record AuthWebResponse(
    string AccessToken,
    string RefreshToken,
    string Message
);

public record UpdateRefreshTokenWebRequest(
    string RefreshToken
);

public record ConfirmEmailWebRequest(
    [Required] string Token
);