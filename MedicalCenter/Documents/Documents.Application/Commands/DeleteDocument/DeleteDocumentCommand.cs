using ErrorOr;
using MediatR;

namespace Documents.Application.Commands.DeleteDocument;

public record DeleteDocumentCommand(Guid Id) : IRequest<ErrorOr<Deleted>>;
