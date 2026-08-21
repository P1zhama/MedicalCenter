using Documents.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace Documents.Infrastructure.Storage;

public sealed class MinioFileStorage : IFileStorage
{
    private readonly IMinioClient _client;
    private readonly string _bucket;

    public MinioFileStorage(IMinioClient client, IOptions<MinioSettings> settings)
    {
        _client = client;
        _bucket = settings.Value.Bucket;
    }

    public async Task SaveAsync(
        string objectKey,
        Stream content,
        string contentType,
        long size,
        CancellationToken cancellationToken = default)
    {
        var args = new PutObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectKey)
            .WithStreamData(content)
            .WithObjectSize(size)
            .WithContentType(contentType);

        await _client.PutObjectAsync(args, cancellationToken);
    }

    public async Task<Stream> OpenReadAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        var buffer = new MemoryStream();

        var args = new GetObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectKey)
            .WithCallbackStream((stream, token) => stream.CopyToAsync(buffer, token));

        await _client.GetObjectAsync(args, cancellationToken);

        buffer.Seek(0, SeekOrigin.Begin);

        return buffer;
    }

    public async Task DeleteAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        var args = new RemoveObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectKey);

        await _client.RemoveObjectAsync(args, cancellationToken);
    }
}
