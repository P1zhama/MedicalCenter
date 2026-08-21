using Common.Abstractions.Providers;
using Common.Abstractions.Security;
using Documents.Application.Common.Behaviors;
using Documents.Application.Common.Dtos;
using Documents.Application.Common.Interfaces;
using Documents.Application.Common.Services;
using Documents.Domain;
using Documents.Domain.Constants;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Documents.Application.Commands.UploadDocument;

public sealed class UploadDocumentCommandHandler
    : IRequestHandler<UploadDocumentCommand, ErrorOr<UploadedDocumentDto>>
{
    private readonly IDocumentCommandRepository _repository;
    private readonly IFileStorage _storage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IGuidProvider _guidProvider;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<UploadDocumentCommandHandler> _logger;

    public UploadDocumentCommandHandler(
        IDocumentCommandRepository repository,
        IFileStorage storage,
        IUnitOfWork unitOfWork,
        ICurrentUserProvider currentUserProvider,
        IGuidProvider guidProvider,
        TimeProvider timeProvider,
        ILogger<UploadDocumentCommandHandler> logger)
    {
        _repository = repository;
        _storage = storage;
        _unitOfWork = unitOfWork;
        _currentUserProvider = currentUserProvider;
        _guidProvider = guidProvider;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<ErrorOr<UploadedDocumentDto>> Handle(
        UploadDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var user = _currentUserProvider.User;

        if (user is null || !user.IsAuthenticated)
            return Error.Unauthorized("Auth.Unauthenticated", "Authentication is required.");

        Guid? ownerProfileId;

        if (request.OwnerProfileId is null || request.OwnerProfileId == user.ProfileId)
            ownerProfileId = user.ProfileId;
        else if (PermissionCheck.Has(user, Permissions.UploadForOthers))
            ownerProfileId = request.OwnerProfileId;
        else
            return Error.Forbidden("Auth.Forbidden", "You are not allowed to perform this action.");

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
                ownerProfileId,
                user.Id!.Value,
                now);

            await _repository.AddAsync(document, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new UploadedDocumentDto(
                document.Id,
                $"/api/documents/{document.Id}",
                document.FileName,
                document.ContentType,
                document.Size);
        }
        catch
        {
            _logger.LogWarning("Removing orphaned object {ObjectKey} after a failed upload.", objectKey);

            await _storage.DeleteAsync(objectKey, CancellationToken.None);

            throw;
        }
    }

    private static async Task<FileSignature?> DetectSignatureAsync(Stream content, CancellationToken cancellationToken)
    {
        var header = new byte[FileSignatureInspector.HeaderLength];

        content.Seek(0, SeekOrigin.Begin);

        var read = await content.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, cancellationToken);

        content.Seek(0, SeekOrigin.Begin);

        return FileSignatureInspector.Detect(header.AsSpan(0, read));
    }
}
