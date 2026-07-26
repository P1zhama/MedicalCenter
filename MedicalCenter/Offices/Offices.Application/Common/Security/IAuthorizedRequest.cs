namespace Offices.Application.Common.Security;

public interface IAuthorizedRequest
{
    string RequiredPermission { get; }
}
