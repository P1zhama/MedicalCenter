using Common.Abstractions.Security;
using Common.Domain;
using ErrorOr;
using MedicalCenter.Shared.Contracts;
using Microsoft.Extensions.Time.Testing;
using Offices.Application.Commands.ChangeOfficeStatus;
using Offices.Application.Common.Interfaces;
using Offices.Domain.Enums;
using Offices.Domain.Models;
using Offices.UnitTests.Support;

namespace Offices.UnitTests.Application.Commands;

public sealed class ChangeOfficeStatusCommandHandlerTests
{
    private static readonly Guid OfficeId = OfficeBuilder.DefaultId;
    private static readonly DateTimeOffset Now = new(2026, 3, 15, 10, 30, 0, TimeSpan.Zero);

    private readonly IOfficeCommandRepository _repository = Substitute.For<IOfficeCommandRepository>();
    private readonly ICurrentUserProvider _currentUserProvider = Substitute.For<ICurrentUserProvider>();
    private readonly ChangeOfficeStatusCommandHandler _handler;
    private Office? _savedOffice;
    private long? _expectedVersion;
    private IReadOnlyCollection<object>? _savedEvents;

    public ChangeOfficeStatusCommandHandlerTests()
    {
        _currentUserProvider.User.Returns(TestUsers.Receptionist);
        _repository
            .UpdateAsync(
                Arg.Do<Office>(office => _savedOffice = office),
                Arg.Do<long>(version => _expectedVersion = version),
                Arg.Do<IReadOnlyCollection<object>>(events => _savedEvents = events),
                Arg.Any<CancellationToken>())
            .Returns(true);

        _handler = new ChangeOfficeStatusCommandHandler(_repository, _currentUserProvider, new FakeTimeProvider(Now));
    }

    private Office SavedOffice
        => _savedOffice ?? throw new InvalidOperationException("The handler did not save the office.");

    private void GivenStoredOffice(Office office)
        => _repository.GetByIdAsync(office.Id, Arg.Any<CancellationToken>()).Returns(office);

    [Fact]
    public async Task Handle_OfficeNotFound_ReturnsNotFoundAndSavesNothing()
    {
        _repository.GetByIdAsync(OfficeId, Arg.Any<CancellationToken>()).Returns((Office?)null);

        var result = await _handler.Handle(
            new ChangeOfficeStatusCommand(OfficeId, OfficeStatus.Inactive),
            TestContext.Current.CancellationToken);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Code.Should().Be("Office.NotFound");
        await _repository.DidNotReceive().UpdateAsync(
            Arg.Any<Office>(),
            Arg.Any<long>(),
            Arg.Any<IReadOnlyCollection<object>>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(OfficeStatus.Active, OfficeStatus.Inactive)]
    [InlineData(OfficeStatus.Inactive, OfficeStatus.Active)]
    public async Task Handle_ExistingOffice_SavesNewStatusAndRecordsEditor(OfficeStatus from, OfficeStatus to)
    {
        GivenStoredOffice(new OfficeBuilder().WithStatus(from).Build());

        var result = await _handler.Handle(new ChangeOfficeStatusCommand(OfficeId, to), TestContext.Current.CancellationToken);

        result.IsError.Should().BeFalse();
        SavedOffice.Status.Should().Be(to);
        SavedOffice.Audit.Should().Be(
            new AuditInfo(OfficeBuilder.DefaultCreatorId, OfficeBuilder.DefaultCreatedAt, TestUsers.ReceptionistId, Now));
    }

    [Fact]
    public async Task Handle_ExistingOffice_ExpectsVersionThatWasRead()
    {
        GivenStoredOffice(new OfficeBuilder().WithVersion(3).Build());

        await _handler.Handle(new ChangeOfficeStatusCommand(OfficeId, OfficeStatus.Inactive), TestContext.Current.CancellationToken);

        _expectedVersion.Should().Be(3);
        SavedOffice.Version.Should().Be(4);
    }

    [Fact]
    public async Task Handle_ActiveOfficeDeactivated_SavesDeactivationEvent()
    {
        GivenStoredOffice(new OfficeBuilder().WithStatus(OfficeStatus.Active).Build());

        await _handler.Handle(new ChangeOfficeStatusCommand(OfficeId, OfficeStatus.Inactive), TestContext.Current.CancellationToken);

        _savedEvents.Should().ContainSingle()
            .Which.Should().Be(new OfficeDeactivatedEvent(OfficeId, Now.UtcDateTime));
    }

    [Theory]
    [InlineData(OfficeStatus.Active, OfficeStatus.Active)]
    [InlineData(OfficeStatus.Inactive, OfficeStatus.Inactive)]
    [InlineData(OfficeStatus.Inactive, OfficeStatus.Active)]
    public async Task Handle_TransitionOtherThanDeactivation_SavesNoEvents(OfficeStatus from, OfficeStatus to)
    {
        GivenStoredOffice(new OfficeBuilder().WithStatus(from).Build());

        await _handler.Handle(new ChangeOfficeStatusCommand(OfficeId, to), TestContext.Current.CancellationToken);

        _savedEvents.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task Handle_OfficeChangedConcurrently_ReturnsConflict()
    {
        GivenStoredOffice(new OfficeBuilder().Build());
        _repository
            .UpdateAsync(
                Arg.Any<Office>(),
                Arg.Any<long>(),
                Arg.Any<IReadOnlyCollection<object>>(),
                Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _handler.Handle(
            new ChangeOfficeStatusCommand(OfficeId, OfficeStatus.Inactive),
            TestContext.Current.CancellationToken);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
        result.FirstError.Code.Should().Be("Office.ConcurrencyConflict");
    }
}
