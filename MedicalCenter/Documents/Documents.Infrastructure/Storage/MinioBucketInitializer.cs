using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace Documents.Infrastructure.Storage;

public sealed class MinioBucketInitializer : IHostedService
{
    private readonly IMinioClient _client;
    private readonly MinioSettings _settings;
    private readonly ILogger<MinioBucketInitializer> _logger;

    public MinioBucketInitializer(
        IMinioClient client,
        IOptions<MinioSettings> settings,
        ILogger<MinioBucketInitializer> logger)
    {
        _client = client;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var exists = await _client.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(_settings.Bucket),
            cancellationToken);

        if (exists)
        {
            _logger.LogInformation("Object storage bucket {Bucket} is ready.", _settings.Bucket);

            return;
        }

        await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(_settings.Bucket), cancellationToken);

        _logger.LogInformation("Object storage bucket {Bucket} was created.", _settings.Bucket);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
