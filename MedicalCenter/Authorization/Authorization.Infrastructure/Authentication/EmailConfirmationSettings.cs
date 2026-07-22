namespace Authorization.Infrastructure.Authentication;

public sealed class EmailConfirmationSettings
{
    public const string SectionName = "EmailConfirmation";

    public int TokenLifetimeHours { get; set; } = 24;
}
