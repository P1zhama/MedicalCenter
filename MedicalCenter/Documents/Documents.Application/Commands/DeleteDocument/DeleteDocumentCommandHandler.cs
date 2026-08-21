using Common.Abstractions.Security;
using Documents.Application.Common.Interfaces;
using Documents.Application.Common.Services;
using ErrorOr;
using MediatR;

namespace Documents.Application.Commands.DeleteDocument;

public sealed class DeleteDocumentCommandHandler
    : IRequestHandler<DeleteDocumentCommand, ErrorOr<Deleted>>
{
    private readonly IDocumentCommandRepository _repository;
    private readonly IFileStorage _storage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserProvider _currentUserProvider;

    public DeleteDocumentCommandHandler(
        IDocumentCommandRepository repository,
        IFileStorage storage,
        IUnitOfWork unitOfWork,
        ICurrentUserProvider currentUserProvider)
    {
        _repository = repository;
        _storage = storage;
        _unitOfWork = unitOfWork;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<ErrorOr<Deleted>> Handle(
        DeleteDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var user = _currentUserProvider.User;

        if (user is null || !user.IsAuthenticated)
            return Error.Unauthorized("Auth.Unauthenticated", "Authentication is required.");

        var document = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (document is null)
            return Error.NotFound("Document.NotFound", "Document was not found.");

        if (!DocumentAccessPolicy.CanDelete(document, user))
            return Error.Forbidden("Auth.Forbidden", "You are not allowed to perform this action.");

        _repository.Remove(document);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _storage.DeleteAsync(document.ObjectKey, cancellationToken);

        return Result.Deleted;
    }
}
