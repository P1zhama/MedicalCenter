using Authorization.Domain.Constants;
using Common.Domain;

namespace Authorization.Domain;

public sealed class AccountClaim : Entity<Guid>
{
    public const int TypeMaxLength = 100;

    public const int ValueMaxLength = 200;

    public Guid AccountId { get; }

    public string Type { get; }

    public string Value { get; }

    public DateTimeOffset CreatedAt { get; }

    private AccountClaim(Guid id, Guid accountId, string type, string value, DateTimeOffset createdAt)
        : base(id)
    {
        AccountId = accountId;
        Type = type;
        Value = value;
        CreatedAt = createdAt;
    }

    public static AccountClaim Role(Guid id, Guid accountId, string role, DateTimeOffset createdAt)
        => Create(id, accountId, AppClaimTypes.Role, role, createdAt);

    public static AccountClaim Restore(Guid id, Guid accountId, string type, string value, DateTimeOffset createdAt)
        => new(id, accountId, type, value, createdAt);

    private static AccountClaim Create(Guid id, Guid accountId, string type, string value, DateTimeOffset createdAt)
    {
        Guard.NotEmpty(id, nameof(id));
        Guard.NotEmpty(accountId, nameof(accountId));

        type = Guard.MaxLength(Guard.NotNullOrWhiteSpace(type, nameof(type)), TypeMaxLength, nameof(type));
        value = Guard.MaxLength(Guard.NotNullOrWhiteSpace(value, nameof(value)), ValueMaxLength, nameof(value));

        return new AccountClaim(id, accountId, type, value, createdAt);
    }
}
