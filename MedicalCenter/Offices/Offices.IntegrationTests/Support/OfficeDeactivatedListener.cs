using System.Threading.Channels;
using MassTransit;
using MedicalCenter.Shared.Contracts;

namespace Offices.IntegrationTests.Support;

public sealed class OfficeDeactivatedListener : IAsyncDisposable
{
    private readonly IBusControl _bus;
    private readonly Channel<OfficeDeactivatedEvent> _received = Channel.CreateUnbounded<OfficeDeactivatedEvent>();

    private OfficeDeactivatedListener(string host, ushort port, string queueName)
    {
        _bus = Bus.Factory.CreateUsingRabbitMq(configurator =>
        {
            configurator.Host(host, port, "/", settings =>
            {
                settings.Username("guest");
                settings.Password("guest");
            });

            configurator.ReceiveEndpoint(queueName, endpoint =>
            {
                endpoint.Handler<OfficeDeactivatedEvent>(context =>
                {
                    _received.Writer.TryWrite(context.Message);

                    return Task.CompletedTask;
                });
            });
        });
    }

    public static async Task<OfficeDeactivatedListener> StartAsync(
        OfficesEnvironment environment,
        CancellationToken cancellationToken)
    {
        var listener = new OfficeDeactivatedListener(
            environment.RabbitMqHost,
            environment.RabbitMqPort,
            $"offices-tests-{Guid.NewGuid():N}");

        await listener._bus.StartAsync(cancellationToken);

        return listener;
    }

    public async Task<OfficeDeactivatedEvent> WaitForEventAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(timeout);

        return await _received.Reader.ReadAsync(deadline.Token);
    }

    public async ValueTask DisposeAsync() => await _bus.StopAsync();
}
