using Authorization.Application.Common.Interfaces;
using Authorization.Application.Common.Models;
using Common.Abstractions.Providers;
using Microsoft.Extensions.Options;

namespace Authorization.Infrastructure.Authentication;

public sealed class EmailConfirmationTokenGenerator : IEmailConfirmationTokenGenerator
{
    private readonly EmailConfirmationSettings _settings;
    private readonly IRandomProvider _randomProvider;
    private readonly ITokenHashGenerator _tokenHashGenerator;

    public EmailConfirmationTokenGenerator(
        IOptions<EmailConfirmationSettings> settings,
        IRandomProvider randomProvider,
        ITokenHashGenerator tokenHashGenerator)
    {
        _settings = settings.Value;
        _randomProvider = randomProvider;
        _tokenHashGenerator = tokenHashGenerator;
    }

    public EmailConfirmationTokenDescriptor Generate(DateTimeOffset issuedAt)
    {
        var token = _randomProvider.GenerateToken();

        return new EmailConfirmationTokenDescriptor(
            token,
            _tokenHashGenerator.Hash(token),
            issuedAt.AddHours(_settings.TokenLifetimeHours));
    }
}
