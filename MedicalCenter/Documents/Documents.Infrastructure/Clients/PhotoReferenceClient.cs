using Documents.Application.Common.Interfaces;
using Profiles.Api.Protos;

namespace Documents.Infrastructure.Clients;

public sealed class PhotoReferenceClient : IPhotoReferenceClient
{
    private readonly ProfilesService.ProfilesServiceClient _client;

    public PhotoReferenceClient(ProfilesService.ProfilesServiceClient client)
    {
        _client = client;
    }

    public async Task<IReadOnlyList<Guid>> GetReferencedAsync(
        IReadOnlyCollection<Guid> documentIds,
        CancellationToken cancellationToken = default)
    {
        var request = new GetReferencedPhotosRequest();

        request.DocumentIds.AddRange(documentIds.Select(id => id.ToString()));

        var response = await _client.GetReferencedPhotosAsync(request, cancellationToken: cancellationToken);

        return response.ReferencedIds
            .Select(Guid.Parse)
            .ToList();
    }
}
