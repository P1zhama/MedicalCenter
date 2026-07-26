using Authorization.Application.Common.Interfaces;
using Authorization.Application.Common.Security;

namespace Authorization.Infrastructure.Security;

public sealed class CurrentUserProvider : ICurrentUserProvider
{
    public CurrentUser? User { get; private set; }

    public void Set(CurrentUser user) => User = user;
}
