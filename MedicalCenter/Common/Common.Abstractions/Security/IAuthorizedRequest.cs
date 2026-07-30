namespace Common.Abstractions.Security;

public interface IAuthorizedRequest
{
    string RequiredPermission { get; }
}
