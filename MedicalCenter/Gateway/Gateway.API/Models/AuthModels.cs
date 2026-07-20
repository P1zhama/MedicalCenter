using System.ComponentModel.DataAnnotations;

namespace Gateway.API.Models;

public record SignUpWebRequest(
    [Required][EmailAddress] string Email,
    [Required][MinLength(6)] string Password
);

public record SignUpWebResponse(string AccountId);
