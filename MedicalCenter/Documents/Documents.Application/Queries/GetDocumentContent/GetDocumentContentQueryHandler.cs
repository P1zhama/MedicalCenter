using Common.Abstractions.Security;
using Documents.Application.Common.Dtos;
using Documents.Application.Common.Services;
using ErrorOr;
using MediatR;

namespace Documents.Application.Queries.GetDocumentContent;

public sealed class GetDocumentContentQueryHandler
    : IRequestHandler<GetDocumentContentQuery, ErrorOr<DocumentContentDto>>
{
    private readonly DocumentStorageFacade _documents;
    private readonly ICurrentUserProvider _currentUserProvider;

    public GetDocumentContentQueryHandler(
        DocumentStorageFacade documents,
        ICurrentUserProvider currentUserProvider)
    {
        _documents = documents;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<ErrorOr<DocumentContentDto>> Handle(
        GetDocumentContentQuery request,
        CancellationToken cancellationToken)
    {
        var document = await _documents.FindAsync(request.Id, cancellationToken);

        if (document is null)
            return Error.NotFound("Document.NotFound", "Document was not found.");

        var user = _currentUserProvider.User;

        if (!DocumentAccessPolicy.CanRead(document, user))
        {
            return user is null || !user.IsAuthenticated
                ? Error.Unauthorized("Auth.Unauthenticated", "Authentication is required.")
                : Error.Forbidden("Auth.Forbidden", "You are not allowed to perform this action.");
        }

        var content = await _documents.OpenAsync(document, cancellationToken);

        return new DocumentContentDto(
            document.Id,
            content,
            document.ContentType,
            document.FileName,
            document.Size,
            document.IsPublic);
    }
}
