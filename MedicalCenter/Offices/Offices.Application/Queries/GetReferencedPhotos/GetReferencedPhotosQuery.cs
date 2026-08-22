using ErrorOr;
using MediatR;

namespace Offices.Application.Queries.GetReferencedPhotos;

public record GetReferencedPhotosQuery(IReadOnlyCollection<Guid> DocumentIds)
    : IRequest<ErrorOr<IReadOnlyList<Guid>>>;
