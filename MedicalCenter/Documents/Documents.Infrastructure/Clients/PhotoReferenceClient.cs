using Documents.Application.Common.Interfaces;
using OfficesProtos = Offices.Api.Protos;
using ProfilesProtos = Profiles.Api.Protos;

namespace Documents.Infrastructure.Clients;

public sealed class PhotoReferenceClient : IPhotoReferenceClient
{
    private readonly ProfilesProtos.ProfilesService.ProfilesServiceClient _profiles;
    private readonly OfficesProtos.OfficesService.OfficesServiceClient _offices;

    public PhotoReferenceClient(
        ProfilesProtos.ProfilesService.ProfilesServiceClient profiles,
        OfficesProtos.OfficesService.OfficesServiceClient offices)
    {
        _profiles = profiles;
        _offices = offices;
    }

    public async Task<IReadOnlyList<Guid>> GetReferencedAsync(
        IReadOnlyCollection<Guid> documentIds,
        CancellationToken cancellationToken = default)
    {
        var ids = documentIds.Select(id => id.ToString()).ToList();

        var profilesRequest = new ProfilesProtos.GetReferencedPhotosRequest();
        profilesRequest.DocumentIds.AddRange(ids);

        var officesRequest = new OfficesProtos.GetReferencedPhotosRequest();
        officesRequest.DocumentIds.AddRange(ids);

        var profilesTask = _profiles.GetReferencedPhotosAsync(profilesRequest, cancellationToken: cancellationToken)
            .ResponseAsync;

        var officesTask = _offices.GetReferencedPhotosAsync(officesRequest, cancellationToken: cancellationToken)
            .ResponseAsync;

        await Task.WhenAll(profilesTask, officesTask);

        var referenced = new HashSet<Guid>();

        foreach (var id in (await profilesTask).ReferencedIds.Concat((await officesTask).ReferencedIds))
        {
            if (Guid.TryParse(id, out var parsed))
                referenced.Add(parsed);
        }

        return referenced.ToList();
    }
}
