using Offices.Application.Common.Interfaces;
using Offices.Application.Common.Security;

namespace Offices.Infrastructure.Security;

public sealed class CurrentUserProvider : ICurrentUserProvider
{
    public CurrentUser? User { get; private set; }

    public void Set(CurrentUser user) => User = user;
}
