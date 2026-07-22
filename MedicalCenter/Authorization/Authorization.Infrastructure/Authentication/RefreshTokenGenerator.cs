using Authorization.Application.Common.Interfaces;
using Authorization.Application.Common.Models;
using Common.Abstractions.Providers;
using Microsoft.Extensions.Options;

namespace Authorization.Infrastructure.Authentication;

public sealed class RefreshTokenGenerator : IRefreshTokenGenerator
{
    private readonly JwtSettings _settings;
    private readonly IRandomProvider _randomProvider;
    private readonly ITokenHashGenerator _tokenHashGenerator;

    public RefreshTokenGenerator(
        IOptions<JwtSettings> settings,
        IRandomProvider randomProvider,
        ITokenHashGenerator tokenHashGenerator)
    {
        _settings = settings.Value;
        _randomProvider = randomProvider;
        _tokenHashGenerator = tokenHashGenerator;
    }

    public RefreshTokenDescriptor Generate(DateTimeOffset issuedAt)
    {
        var token = _randomProvider.GenerateToken();

        return new RefreshTokenDescriptor(
            token,
            _tokenHashGenerator.Hash(token),
            issuedAt.AddDays(_settings.RefreshTokenLifetimeDays));
    }
}
