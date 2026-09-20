using Microsoft.Extensions.DependencyInjection;
using Offices.Application.Common.Dtos;
using Offices.Application.Common.Interfaces;
using Offices.Domain.Enums;
using Offices.Domain.Models;
using Offices.IntegrationTests.Support;

namespace Offices.IntegrationTests.Infrastructure;

public sealed class OfficeQueryRepositoryTests : IAsyncLifetime
{
    private readonly OfficesServices _services;

    public OfficeQueryRepositoryTests(OfficesEnvironment environment)
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

    private async Task<T> WithRepository<T>(Func<IOfficeQueryRepository, Task<T>> action)
    {
        await using var scope = _services.CreateScope();

        return await action(scope.ServiceProvider.GetRequiredService<IOfficeQueryRepository>());
    }

    [Fact]
    public async Task GetByIdAsync_StoredOffice_ReturnsDtoWithFormattedAddress()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var office = TestOffice.Create();
        await Seed(cancellationToken, office);

        var dto = await WithRepository(repository => repository.GetByIdAsync(office.Id, cancellationToken));

        dto.Should().Be(new OfficeDto(
            office.Id,
            "/api/documents/7c1d2e3f-4a5b-4c6d-8e9f-0a1b2c3d4e03",
            "Москва, Тверская, 12, 101",
            "Москва",
            "Тверская",
            "12",
            "101",
            "Active",
            "+74951234567"));
    }

    [Fact]
    public async Task GetByIdAsync_OfficeWithoutOptionalFields_ReturnsNullsAndShortAddress()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var office = TestOffice.Create(officeNumber: null, photoUrl: null);
        await Seed(cancellationToken, office);

        var dto = await WithRepository(repository => repository.GetByIdAsync(office.Id, cancellationToken));

        dto!.PhotoUrl.Should().BeNull();
        dto.OfficeNumber.Should().BeNull();
        dto.Address.Should().Be("Москва, Тверская, 12");
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var dto = await WithRepository(repository => repository.GetByIdAsync(Guid.NewGuid(), cancellationToken));

        dto.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ActiveAndInactiveOffices_ReturnsEveryOffice()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var active = TestOffice.Create();
        var inactive = TestOffice.Create(
            city: "Казань",
            street: "Баумана",
            houseNumber: "5",
            officeNumber: null,
            status: OfficeStatus.Inactive);
        await Seed(cancellationToken, active, inactive);

        var offices = await WithRepository(repository => repository.GetAllAsync(cancellationToken));

        offices.Should().BeEquivalentTo(
        [
            new OfficeListItemDto(active.Id, "Москва, Тверская, 12, 101", "Active", "+74951234567"),
            new OfficeListItemDto(inactive.Id, "Казань, Баумана, 5", "Inactive", "+74951234567")
        ]);
    }

    [Fact]
    public async Task GetActiveAsync_MixedStatuses_ReturnsOnlyActiveOrderedByAddress()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var moscow = TestOffice.Create();
        var abakan = TestOffice.Create(
            city: "Абакан",
            street: "Ленина",
            houseNumber: "3",
            officeNumber: null,
            photoUrl: null);
        var closed = TestOffice.Create(
            city: "Казань",
            street: "Баумана",
            houseNumber: "5",
            status: OfficeStatus.Inactive);
        await Seed(cancellationToken, moscow, abakan, closed);

        var offices = await WithRepository(repository => repository.GetActiveAsync(cancellationToken));

        offices.Should().Equal(
            new PublicOfficeDto(abakan.Id, "Абакан, Ленина, 3", null, "+74951234567"),
            new PublicOfficeDto(
                moscow.Id,
                "Москва, Тверская, 12, 101",
                "/api/documents/7c1d2e3f-4a5b-4c6d-8e9f-0a1b2c3d4e03",
                "+74951234567"));
    }

    [Theory]
    [InlineData(OfficeStatus.Active, true)]
    [InlineData(OfficeStatus.Inactive, false)]
    public async Task IsActiveAsync_StoredOffice_ReflectsItsStatus(OfficeStatus status, bool expected)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var office = TestOffice.Create(status: status);
        await Seed(cancellationToken, office);

        var isActive = await WithRepository(repository => repository.IsActiveAsync(office.Id, cancellationToken));

        isActive.Should().Be(expected);
    }

    [Fact]
    public async Task IsActiveAsync_UnknownId_ReturnsFalse()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var isActive = await WithRepository(repository => repository.IsActiveAsync(Guid.NewGuid(), cancellationToken));

        isActive.Should().BeFalse();
    }
}
