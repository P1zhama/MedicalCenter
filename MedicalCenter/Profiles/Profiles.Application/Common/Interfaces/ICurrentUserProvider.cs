using Profiles.Application.Common.Security;

namespace Profiles.Application.Common.Interfaces;

public interface ICurrentUserProvider
{
    CurrentUser? User { get; }
}
