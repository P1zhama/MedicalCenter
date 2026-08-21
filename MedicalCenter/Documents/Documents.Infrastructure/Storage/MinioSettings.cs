namespace Documents.Infrastructure.Storage;

public sealed class MinioSettings
{
    public const string SectionName = "MinioSettings";

    public string Endpoint { get; set; } = string.Empty;

    public string AccessKey { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public string Bucket { get; set; } = string.Empty;

    public bool UseSsl { get; set; }
}
