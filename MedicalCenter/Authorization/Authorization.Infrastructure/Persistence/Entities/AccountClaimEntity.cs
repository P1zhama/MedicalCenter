namespace Authorization.Infrastructure.Persistence.Entities;

public class AccountClaimEntity
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    public string Type { get; set; } = null!;

    public string Value { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public AccountEntity Account { get; set; } = null!;
}
