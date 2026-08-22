using System.Text.RegularExpressions;
using ErrorOr;
using MediatR;
using Offices.Application.Common.Interfaces;

namespace Offices.Application.Queries.GetReferencedPhotos;

public sealed partial class GetReferencedPhotosQueryHandler
    : IRequestHandler<GetReferencedPhotosQuery, ErrorOr<IReadOnlyList<Guid>>>
{
    private static readonly IReadOnlyList<Guid> None = [];

    private readonly IPhotoReferenceRepository _repository;

    public GetReferencedPhotosQueryHandler(IPhotoReferenceRepository repository)
    {
        _repository = repository;
    }

    public async Task<ErrorOr<IReadOnlyList<Guid>>> Handle(
        GetReferencedPhotosQuery request,
        CancellationToken cancellationToken)
    {
        if (request.DocumentIds.Count == 0)
            return ErrorOrFactory.From(None);

        var photoUrls = await _repository.GetPhotoUrlsAsync(cancellationToken);

        var referenced = new HashSet<Guid>();

        foreach (var url in photoUrls)
        {
            foreach (Match match in GuidPattern().Matches(url))
            {
                if (Guid.TryParse(match.Value, out var parsed))
                    referenced.Add(parsed);
            }
        }

        IReadOnlyList<Guid> matches = request.DocumentIds.Where(referenced.Contains).ToList();

        return ErrorOrFactory.From(matches);
    }

    [GeneratedRegex(
        "[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}",
        RegexOptions.Compiled)]
    private static partial Regex GuidPattern();
}
