using Common.Abstractions.Providers;

namespace Common.Infrastructure.Providers;

public sealed class GuidProvider : IGuidProvider
{
    public Guid NewGuid() => Guid.NewGuid();
}
