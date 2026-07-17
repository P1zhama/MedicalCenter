
namespace Authorization.Application.Common.Dtos;

public record AuthResultDto
(
    string AccessToken,
    string RefreshToken,
    string Message
);