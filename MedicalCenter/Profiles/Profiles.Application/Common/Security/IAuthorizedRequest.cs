namespace Profiles.Application.Common.Security;

public interface IAuthorizedRequest
{
    string RequiredPermission { get; }
}
