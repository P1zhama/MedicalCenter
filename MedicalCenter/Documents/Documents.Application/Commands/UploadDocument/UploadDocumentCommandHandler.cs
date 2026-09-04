using Common.Abstractions.Security;
using Documents.Application.Common.Behaviors;
using Documents.Application.Common.Dtos;
using Documents.Application.Common.Services;
using Documents.Domain.Constants;
using ErrorOr;
using MediatR;

namespace Documents.Application.Commands.UploadDocument;

public sealed class UploadDocumentCommandHandler
    : IRequestHandler<UploadDocumentCommand, ErrorOr<UploadedDocumentDto>>
{
    private readonly DocumentStorageFacade _documents;
    private readonly ICurrentUserProvider _currentUserProvider;

    public UploadDocumentCommandHandler(
        DocumentStorageFacade documents,
        ICurrentUserProvider currentUserProvider)
    {
        _documents = documents;
        _currentUserProvider = currentUserProvider;
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

        var stored = await _documents.StoreAsync(
            new DocumentUploadRequest(
                request.Content,
                request.FileName,
                request.Size,
                request.Kind,
                ownerProfileId,
                user.Id!.Value),
            cancellationToken);

        if (stored.IsError)
            return stored.Errors;

        var document = stored.Value;

        return new UploadedDocumentDto(
            document.Id,
            $"/api/documents/{document.Id}",
            document.FileName,
            document.ContentType,
            document.Size);
    }
}
