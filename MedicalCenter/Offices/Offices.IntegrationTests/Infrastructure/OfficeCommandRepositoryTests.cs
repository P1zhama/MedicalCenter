using Common.Domain;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using Offices.Application.Common.Events;
using Offices.Application.Common.Interfaces;
using Offices.Domain.Enums;
using Offices.Domain.Models;
using Offices.Domain.ValueObjects;
using Offices.IntegrationTests.Support;

namespace Offices.IntegrationTests.Infrastructure;

public sealed class OfficeCommandRepositoryTests : IAsyncLifetime
{
    private readonly OfficesServices _services;

    public OfficeCommandRepositoryTests(OfficesEnvironment environment)
    {
        _services = OfficesServices.Create(environment.MongoConnectionString, $"offices-{Guid.NewGuid():N}");
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public ValueTask DisposeAsync() => _services.DisposeAsync();

    private async Task<T> WithRepository<T>(Func<IOfficeCommandRepository, Task<T>> action)
    {
        await using var scope = _services.CreateScope();

        return await action(scope.ServiceProvider.GetRequiredService<IOfficeCommandRepository>());
    }

    private Task Add(Office office, CancellationToken cancellationToken)
        => WithRepository(async repository =>
        {
            await repository.AddAsync(office, cancellationToken);
            return true;
        });

    private Task<Office?> Read(Guid id, CancellationToken cancellationToken)
        => WithRepository(repository => repository.GetByIdAsync(id, cancellationToken));

    private Task<List<BsonDocument>> ReadOutbox(CancellationToken cancellationToken)
        => _services.OutboxMessages.Find(FilterDefinition<BsonDocument>.Empty).ToListAsync(cancellationToken);

    [Fact]
    public async Task AddAsync_NewOffice_IsReadBackWithEveryField()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var office = TestOffice.Create();

        await Add(office, cancellationToken);

        var stored = await Read(office.Id, cancellationToken);
        stored.Should().BeEquivalentTo(office, options => options.ComparingByMembers<Office>());
    }

    [Fact]
    public async Task AddAsync_OfficeWithoutOptionalFields_IsReadBackWithNulls()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var office = TestOffice.Create(officeNumber: null, photoUrl: null);

        await Add(office, cancellationToken);

        var stored = await Read(office.Id, cancellationToken);
        stored!.PhotoUrl.Should().BeNull();
        stored.Address.OfficeNumber.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var stored = await Read(Guid.NewGuid(), cancellationToken);

        stored.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ExpectedVersion_SavesChangesAndKeepsOutboxEmpty()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var office = TestOffice.Create();
        await Add(office, cancellationToken);

        var stored = await Read(office.Id, cancellationToken);
        stored!.Update(
            Address.Create("Казань", "Баумана", "5", null),
            "+78435556677",
            null,
            OfficeStatus.Active,
            TestOffice.ReceptionistId,
            TestOffice.UpdatedAt);
        var updated = await WithRepository(repository =>
            repository.UpdateAsync(stored, expectedVersion: 1, [], cancellationToken));

        updated.Should().BeTrue();
        var reread = await Read(office.Id, cancellationToken);
        reread!.Address.Should().Be(Address.Create("Казань", "Баумана", "5", null));
        reread.Version.Should().Be(2);
        reread.Audit.Should().Be(
            new AuditInfo(TestOffice.ReceptionistId, TestOffice.CreatedAt, TestOffice.ReceptionistId, TestOffice.UpdatedAt));
        (await ReadOutbox(cancellationToken)).Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_StaleVersion_KeepsStoredOfficeUntouched()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var office = TestOffice.Create();
        await Add(office, cancellationToken);
        var readByFirstUser = await Read(office.Id, cancellationToken);
        var readBySecondUser = await Read(office.Id, cancellationToken);

        readByFirstUser!.ChangeStatus(OfficeStatus.Inactive, TestOffice.ReceptionistId, TestOffice.UpdatedAt);
        var firstSaved = await WithRepository(repository =>
            repository.UpdateAsync(readByFirstUser, expectedVersion: 1, [], cancellationToken));

        readBySecondUser!.Update(
            Address.Create("Казань", "Баумана", "5", null),
            "+78435556677",
            null,
            OfficeStatus.Active,
            TestOffice.ReceptionistId,
            TestOffice.UpdatedAt);
        var secondSaved = await WithRepository(repository =>
            repository.UpdateAsync(readBySecondUser, expectedVersion: 1, [], cancellationToken));

        firstSaved.Should().BeTrue();
        secondSaved.Should().BeFalse();

        var stored = await Read(office.Id, cancellationToken);
        stored!.Status.Should().Be(OfficeStatus.Inactive);
        stored.Address.Should().Be(Address.Create("Москва", "Тверская", "12", "101"));
        stored.Version.Should().Be(2);
    }

    [Fact]
    public async Task UpdateAsync_Deactivation_StoresEventInOutbox()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var office = TestOffice.Create();
        await Add(office, cancellationToken);

        var stored = await Read(office.Id, cancellationToken);
        var deactivated = stored!.ChangeStatus(OfficeStatus.Inactive, TestOffice.ReceptionistId, TestOffice.UpdatedAt);
        var events = OfficeIntegrationEvents.ForDeactivation(deactivated, stored, TestOffice.UpdatedAt);
        var updated = await WithRepository(repository =>
            repository.UpdateAsync(stored, expectedVersion: 1, events, cancellationToken));

        updated.Should().BeTrue();
        var outbox = await ReadOutbox(cancellationToken);
        outbox.Should().ContainSingle()
            .Which.ToJson().Should().Contain("OfficeDeactivatedEvent");
    }

    [Fact]
    public async Task UpdateAsync_StaleVersionWithDeactivation_StoresNothingInOutbox()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var office = TestOffice.Create();
        await Add(office, cancellationToken);
        var stored = await Read(office.Id, cancellationToken);

        var deactivated = stored!.ChangeStatus(OfficeStatus.Inactive, TestOffice.ReceptionistId, TestOffice.UpdatedAt);
        var events = OfficeIntegrationEvents.ForDeactivation(deactivated, stored, TestOffice.UpdatedAt);
        var updated = await WithRepository(repository =>
            repository.UpdateAsync(stored, expectedVersion: 42, events, cancellationToken));

        updated.Should().BeFalse();
        (await ReadOutbox(cancellationToken)).Should().BeEmpty();
        var reread = await Read(office.Id, cancellationToken);
        reread!.Status.Should().Be(OfficeStatus.Active);
    }
}
