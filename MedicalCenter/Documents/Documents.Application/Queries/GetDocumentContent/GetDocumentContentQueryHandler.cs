using Common.Abstractions.Security;
using Documents.Application.Common.Dtos;
using Documents.Application.Common.Interfaces;
using Documents.Application.Common.Services;
using ErrorOr;
using MediatR;

namespace Documents.Application.Queries.GetDocumentContent;

public sealed class GetDocumentContentQueryHandler
    : IRequestHandler<GetDocumentContentQuery, ErrorOr<DocumentContentDto>>
{
    private readonly IDocumentQueryRepository _repository;
    private readonly IFileStorage _storage;
    private readonly ICurrentUserProvider _currentUserProvider;

    public GetDocumentContentQueryHandler(
        IDocumentQueryRepository repository,
        IFileStorage storage,
        ICurrentUserProvider currentUserProvider)
    {
        _repository = repository;
        _storage = storage;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<ErrorOr<DocumentContentDto>> Handle(
        GetDocumentContentQuery request,
        CancellationToken cancellationToken)
    {
        var document = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (document is null)
            return Error.NotFound("Document.NotFound", "Document was not found.");

        var user = _currentUserProvider.User;

        if (!DocumentAccessPolicy.CanRead(document, user))
        {
            return user is null || !user.IsAuthenticated
                ? Error.Unauthorized("Auth.Unauthenticated", "Authentication is required.")
                : Error.Forbidden("Auth.Forbidden", "You are not allowed to perform this action.");
        }

        var content = await _storage.OpenReadAsync(document.ObjectKey, cancellationToken);

        return new DocumentContentDto(
            document.Id,
            content,
            document.ContentType,
            document.FileName,
            document.Size,
            document.IsPublic);
    }
}
