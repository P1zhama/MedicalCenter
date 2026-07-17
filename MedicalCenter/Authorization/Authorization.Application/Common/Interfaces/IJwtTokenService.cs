using Authorization.Domain;

namespace Authorization.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(Account account);
    string GenerateRefreshToken();


    string GenerateEmailConfirmationToken(string email);
    string? ValidateEmailConfirmationToken(string token);
}