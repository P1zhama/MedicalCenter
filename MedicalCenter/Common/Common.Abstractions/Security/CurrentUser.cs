namespace Common.Abstractions.Security;

public sealed class CurrentUser
{
    public CurrentUser(
        Guid? id,
        Guid? profileId,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions)
    {
        Id = id;
        ProfileId = profileId;
        Roles = roles;
        Permissions = permissions;
    }

    public Guid? Id { get; }

    public Guid? ProfileId { get; }

    public IReadOnlyCollection<string> Roles { get; }

    public IReadOnlyCollection<string> Permissions { get; }

    public bool IsAuthenticated => Id.HasValue;

    public bool HasProfile => ProfileId.HasValue;
}
