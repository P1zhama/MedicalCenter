namespace Common.Abstractions.Security;

public interface ICurrentUserProvider
{
    CurrentUser? User { get; }
}
