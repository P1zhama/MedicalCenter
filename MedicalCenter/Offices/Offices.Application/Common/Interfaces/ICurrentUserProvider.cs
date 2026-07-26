using Offices.Application.Common.Security;

namespace Offices.Application.Common.Interfaces;

public interface ICurrentUserProvider
{
    CurrentUser? User { get; }
}
