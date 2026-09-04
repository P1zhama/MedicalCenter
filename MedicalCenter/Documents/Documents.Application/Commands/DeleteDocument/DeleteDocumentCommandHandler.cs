using Common.Abstractions.Security;
using Documents.Application.Common.Services;
using ErrorOr;
using MediatR;

namespace Documents.Application.Commands.DeleteDocument;

public sealed class DeleteDocumentCommandHandler
    : IRequestHandler<DeleteDocumentCommand, ErrorOr<Deleted>>
{
    private readonly DocumentStorageFacade _documents;
    private readonly ICurrentUserProvider _currentUserProvider;

    public DeleteDocumentCommandHandler(
        DocumentStorageFacade documents,
        ICurrentUserProvider currentUserProvider)
    {
        _documents = documents;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<ErrorOr<Deleted>> Handle(
        DeleteDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var user = _currentUserProvider.User;

        if (user is null || !user.IsAuthenticated)
            return Error.Unauthorized("Auth.Unauthenticated", "Authentication is required.");

        var document = await _documents.FindAsync(request.Id, cancellationToken);

        if (document is null)
            return Error.NotFound("Document.NotFound", "Document was not found.");

        if (!DocumentAccessPolicy.CanDelete(document, user))
            return Error.Forbidden("Auth.Forbidden", "You are not allowed to perform this action.");

        await _documents.RemoveAsync(document, cancellationToken);

        return Result.Deleted;
    }
}
