namespace Authorization.Infrastructure.Notifications;

public sealed class EmailSettings
{
    public const string SectionName = "EmailSettings";

    public string Server { get; set; } = string.Empty;

    public int Port { get; set; }

    public string SenderEmail { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string SenderName { get; set; } = string.Empty;

    public bool UseSsl { get; set; }

    public string ClientAppBaseUrl { get; set; } = string.Empty;
}
