using MedicalCenter.Shared.Contracts;
using Offices.Application.Common.Events;
using Offices.UnitTests.Support;

namespace Offices.UnitTests.Application.Events;

public sealed class OfficeIntegrationEventsTests
{
    private static readonly DateTimeOffset Now = new(2026, 3, 15, 10, 30, 0, TimeSpan.Zero);

    [Fact]
    public void ForDeactivation_OfficeNotDeactivated_ReturnsNoEvents()
    {
        var office = new OfficeBuilder().Build();

        var events = OfficeIntegrationEvents.ForDeactivation(false, office, Now);

        events.Should().BeEmpty();
    }

    [Fact]
    public void ForDeactivation_OfficeDeactivated_ReturnsSingleDeactivatedEvent()
    {
        var office = new OfficeBuilder().Build();

        var events = OfficeIntegrationEvents.ForDeactivation(true, office, Now);

        events.Should().ContainSingle()
            .Which.Should().Be(new OfficeDeactivatedEvent(office.Id, Now.UtcDateTime));
    }

    [Fact]
    public void ForDeactivation_TimeWithOffset_ReportsSameMomentInUtc()
    {
        var office = new OfficeBuilder().Build();
        var moscowTime = new DateTimeOffset(2026, 3, 15, 13, 30, 0, TimeSpan.FromHours(3));

        var events = OfficeIntegrationEvents.ForDeactivation(true, office, moscowTime);

        var deactivatedAt = events.Should().ContainSingle()
            .Which.Should().BeOfType<OfficeDeactivatedEvent>()
            .Which.DeactivatedAt;
        deactivatedAt.Should().Be(new DateTime(2026, 3, 15, 10, 30, 0, DateTimeKind.Utc));
        deactivatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }
}
