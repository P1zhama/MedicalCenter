using ErrorOr;
using MediatR;

namespace Profiles.Application.Queries.GetReferencedPhotos;

public record GetReferencedPhotosQuery(IReadOnlyCollection<Guid> DocumentIds)
    : IRequest<ErrorOr<IReadOnlyList<Guid>>>;
