using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Authorization.Application.Common.Interfaces;
using Authorization.Application.Common.Models;
using Authorization.Domain;
using Common.Abstractions.Providers;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Authorization.Infrastructure.Authentication;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _settings;
    private readonly SigningCredentials _signingCredentials;
    private readonly TimeProvider _timeProvider;
    private readonly IGuidProvider _guidProvider;

    public JwtTokenGenerator(
        IOptions<JwtSettings> settings,
        SigningCredentials signingCredentials,
        TimeProvider timeProvider,
        IGuidProvider guidProvider)
    {
        _settings = settings.Value;
        _signingCredentials = signingCredentials;
        _timeProvider = timeProvider;
        _guidProvider = guidProvider;
    }

    public AccessToken Generate(Account account)
    {
        var issuedAt = _timeProvider.GetUtcNow();
        var expiresAt = issuedAt.AddMinutes(_settings.AccessTokenLifetimeMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, account.Email.Value),
            new(JwtRegisteredClaimNames.Jti, _guidProvider.NewGuid().ToString())
        };

        claims.AddRange(account.Claims.Select(claim => new Claim(claim.Type, claim.Value)));

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: issuedAt.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: _signingCredentials);

        var value = new JwtSecurityTokenHandler().WriteToken(token);

        return new AccessToken(value, expiresAt);
    }
}
