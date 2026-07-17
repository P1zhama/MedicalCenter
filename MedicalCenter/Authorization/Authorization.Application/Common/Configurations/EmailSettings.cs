namespace Authorization.Application.Common.Configurations;

public class EmailSettings
{
    public const string SectionName = "EmailSettings";

    public string Provider { get; set; } = string.Empty;
    public string Server { get; set; } = string.Empty;
    public int Port { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool UseSsl { get; set; } = false;

    public string ClientAppBaseUrl { get; set; } = string.Empty;
    public string InternalAppBaseUrl { get; set; } = string.Empty;
}