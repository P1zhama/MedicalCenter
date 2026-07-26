namespace Authorization.Application.Common.Security;

public interface IAuthorizedRequest
{
    string RequiredPermission { get; }
}
