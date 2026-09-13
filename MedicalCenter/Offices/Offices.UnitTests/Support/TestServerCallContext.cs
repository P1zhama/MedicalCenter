using System.Runtime.CompilerServices;
using Grpc.Core;

namespace Offices.UnitTests.Support;

public sealed class TestServerCallContext : ServerCallContext
{
    private TestServerCallContext(string method, CancellationToken cancellationToken)
    {
        MethodCore = method;
        CancellationTokenCore = cancellationToken;
    }

    public static TestServerCallContext Create(
        string method = "/offices.OfficesService/Test",
        CancellationToken cancellationToken = default)
        => new(method, cancellationToken);

    protected override string MethodCore { get; }

    protected override CancellationToken CancellationTokenCore { get; }

    protected override string HostCore => throw NotUsed();

    protected override string PeerCore => throw NotUsed();

    protected override DateTime DeadlineCore => throw NotUsed();

    protected override Metadata RequestHeadersCore => throw NotUsed();

    protected override Metadata ResponseTrailersCore => throw NotUsed();

    protected override Status StatusCore
    {
        get => throw NotUsed();
        set => throw NotUsed();
    }

    protected override WriteOptions? WriteOptionsCore
    {
        get => throw NotUsed();
        set => throw NotUsed();
    }

    protected override AuthContext AuthContextCore => throw NotUsed();

    protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options)
        => throw NotUsed();

    protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
        => throw NotUsed();

    private static NotSupportedException NotUsed([CallerMemberName] string member = "")
        => new($"{member} is not used by the Offices gRPC code; implement it in {nameof(TestServerCallContext)} when a test needs it.");
}
