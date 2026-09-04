using Common.Abstractions.Providers;
using Documents.Application.Common.Interfaces;
using Documents.Domain;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace Documents.Application.Common.Services;

public sealed class DocumentStorageFacade
{
    private readonly IDocumentCommandRepository _commandRepository;
    private readonly IDocumentQueryRepository _queryRepository;
    private readonly IFileStorage _storage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGuidProvider _guidProvider;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<DocumentStorageFacade> _logger;

    public DocumentStorageFacade(
        IDocumentCommandRepository commandRepository,
        IDocumentQueryRepository queryRepository,
        IFileStorage storage,
        IUnitOfWork unitOfWork,
        IGuidProvider guidProvider,
        TimeProvider timeProvider,
        ILogger<DocumentStorageFacade> logger)
    {
        _commandRepository = commandRepository;
        _queryRepository = queryRepository;
        _storage = storage;
        _unitOfWork = unitOfWork;
        _guidProvider = guidProvider;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<ErrorOr<StoredDocument>> StoreAsync(
        DocumentUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        var signature = await DetectSignatureAsync(request.Content, cancellationToken);

        if (signature is null)
            return Error.Validation("File", "Only JPEG, PNG and WEBP images are allowed.");

        var id = _guidProvider.NewGuid();
        var now = _timeProvider.GetUtcNow();
        var objectKey = ObjectKeys.ForPhoto(id, signature.Extension, now);
        var fileName = Path.GetFileName(request.FileName);

        await _storage.SaveAsync(objectKey, request.Content, signature.ContentType, request.Size, cancellationToken);

        try
        {
            var document = StoredDocument.Create(
                id,
                objectKey,
                fileName,
                signature.ContentType,
                request.Size,
                request.Kind,
                request.OwnerProfileId,
                request.UploadedBy,
                now);

            await _commandRepository.AddAsync(document, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return document;
        }
        catch
        {
            _logger.LogWarning("Removing orphaned object {ObjectKey} after a failed upload.", objectKey);

            await _storage.DeleteAsync(objectKey, CancellationToken.None);

            throw;
        }
    }

    public Task<StoredDocument?> FindAsync(Guid id, CancellationToken cancellationToken = default)
        => _queryRepository.GetByIdAsync(id, cancellationToken);

    public Task<Stream> OpenAsync(StoredDocument document, CancellationToken cancellationToken = default)
        => _storage.OpenReadAsync(document.ObjectKey, cancellationToken);

    public async Task RemoveAsync(StoredDocument document, CancellationToken cancellationToken = default)
    {
        await _storage.DeleteAsync(document.ObjectKey, cancellationToken);

        _commandRepository.Remove(document);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static async Task<FileSignature?> DetectSignatureAsync(
        Stream content,
        CancellationToken cancellationToken)
    {
        var header = new byte[FileSignatureInspector.HeaderLength];

        content.Seek(0, SeekOrigin.Begin);

        var read = await content.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, cancellationToken);

        content.Seek(0, SeekOrigin.Begin);

        return FileSignatureInspector.Detect(header.AsSpan(0, read));
    }
}
