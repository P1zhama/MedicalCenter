using Profiles.Application.Common.Interfaces;
using Profiles.Application.Common.Security;

namespace Profiles.Infrastructure.Security;

public sealed class CurrentUserProvider : ICurrentUserProvider
{
    public CurrentUser? User { get; private set; }

    public void Set(CurrentUser user) => User = user;
}
