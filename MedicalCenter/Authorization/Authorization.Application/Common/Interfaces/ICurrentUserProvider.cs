using Authorization.Application.Common.Security;

namespace Authorization.Application.Common.Interfaces;

public interface ICurrentUserProvider
{
    CurrentUser? User { get; }
}
