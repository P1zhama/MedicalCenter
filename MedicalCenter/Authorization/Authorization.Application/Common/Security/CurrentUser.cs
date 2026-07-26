namespace Authorization.Application.Common.Security;

public sealed class CurrentUser
{
    public CurrentUser(Guid? id, IReadOnlyCollection<string> roles, IReadOnlyCollection<string> permissions)
    {
        Id = id;
        Roles = roles;
        Permissions = permissions;
    }

    public Guid? Id { get; }

    public IReadOnlyCollection<string> Roles { get; }

    public IReadOnlyCollection<string> Permissions { get; }

    public bool IsAuthenticated => Id.HasValue;
}
