using Microsoft.Extensions.DependencyInjection;
using Offices.Application.Common.Interfaces;
using Offices.Domain.Models;
using Offices.IntegrationTests.Support;

namespace Offices.IntegrationTests.Infrastructure;

public sealed class PhotoReferenceRepositoryTests : IAsyncLifetime
{
    private const string SharedPhotoUrl = "/api/documents/7c1d2e3f-4a5b-4c6d-8e9f-0a1b2c3d4e03";
    private const string OtherPhotoUrl = "/api/documents/9d0e1f2a-3b4c-4d5e-8f6a-7b8c9d0e1f15";

    private readonly OfficesServices _services;

    public PhotoReferenceRepositoryTests(OfficesEnvironment environment)
    {
        _services = OfficesServices.Create(environment.MongoConnectionString, $"offices-{Guid.NewGuid():N}");
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public ValueTask DisposeAsync() => _services.DisposeAsync();

    private async Task Seed(CancellationToken cancellationToken, params Office[] offices)
    {
        await using var scope = _services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOfficeCommandRepository>();

        foreach (var office in offices)
            await repository.AddAsync(office, cancellationToken);
    }

    private async Task<IReadOnlyList<string>> ReadPhotoUrls(CancellationToken cancellationToken)
    {
        await using var scope = _services.CreateScope();

        return await scope.ServiceProvider
            .GetRequiredService<IPhotoReferenceRepository>()
            .GetPhotoUrlsAsync(cancellationToken);
    }

    [Fact]
    public async Task GetPhotoUrlsAsync_NoOffices_ReturnsEmpty()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var photoUrls = await ReadPhotoUrls(cancellationToken);

        photoUrls.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPhotoUrlsAsync_OfficesWithAndWithoutPhotos_ReturnsDistinctUrls()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await Seed(
            cancellationToken,
            TestOffice.Create(photoUrl: SharedPhotoUrl),
            TestOffice.Create(city: "Казань", street: "Баумана", houseNumber: "5", photoUrl: SharedPhotoUrl),
            TestOffice.Create(city: "Абакан", street: "Ленина", houseNumber: "3", photoUrl: OtherPhotoUrl),
            TestOffice.Create(city: "Омск", street: "Мира", houseNumber: "7", photoUrl: null));

        var photoUrls = await ReadPhotoUrls(cancellationToken);

        photoUrls.Should().BeEquivalentTo([SharedPhotoUrl, OtherPhotoUrl]);
    }
}
