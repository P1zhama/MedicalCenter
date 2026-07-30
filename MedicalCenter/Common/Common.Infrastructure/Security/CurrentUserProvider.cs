using Common.Abstractions.Security;

namespace Common.Infrastructure.Security;

public sealed class CurrentUserProvider : ICurrentUserProvider
{
    public CurrentUser? User { get; private set; }

    public void Set(CurrentUser user) => User = user;
}
