using Documents.Application.Common.Dtos;
using ErrorOr;
using MediatR;

namespace Documents.Application.Queries.GetDocumentContent;

public record GetDocumentContentQuery(Guid Id) : IRequest<ErrorOr<DocumentContentDto>>;
