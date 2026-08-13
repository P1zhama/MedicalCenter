namespace Common.Api.Authentication;

public sealed class JwtValidationSettings
{
    public const string SectionName = "JwtSettings";

    public string PublicKey { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public int ClockSkewSeconds { get; set; } = 30;
}
