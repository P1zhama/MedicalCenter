using Authorization.Domain.Enums;
using Authorization.Domain.Events;
using Authorization.Domain.ValueObjects;
using Common.Domain;
using ErrorOr;

namespace Authorization.Domain;

public sealed class Account : AggregateRoot<Guid>
{
    public Email Email { get; private set; }

    public string PasswordHash { get; private set; }

    public AccountStatus Status { get; private set; }

    public DateTimeOffset? EmailConfirmedAt { get; private set; }

    public bool IsEmailConfirmed => EmailConfirmedAt.HasValue;

    private Account(
        Guid id,
        Email email,
        string passwordHash,
        AccountStatus status,
        DateTimeOffset? emailConfirmedAt,
        long version,
        AuditInfo audit)
        : base(id, version, audit)
    {
        Email = email;
        PasswordHash = passwordHash;
        Status = status;
        EmailConfirmedAt = emailConfirmedAt;
    }

    public static ErrorOr<Account> CreateNew(
        Guid id,
        string email,
        string passwordHash,
        Guid createdBy,
        DateTimeOffset createdAt)
    {
        var emailResult = Email.Create(email);
        if (emailResult.IsError)
            return emailResult.Errors;

        if (string.IsNullOrWhiteSpace(passwordHash))
            return Error.Validation("Account.PasswordHash", "Password hash must not be empty.");

        var account = new Account(
            id,
            emailResult.Value,
            passwordHash,
            AccountStatus.PendingConfirmation,
            emailConfirmedAt: null,
            version: 1,
            new AuditInfo(createdBy, createdAt, null, null));

        account.AddDomainEvent(new SignUpDomainEvent(account.Id, account.Email.Value, createdAt));

        return account;
    }

    public ErrorOr<Success> ConfirmEmail(DateTimeOffset confirmedAt, Guid updatedBy)
    {
        if (IsEmailConfirmed)
            return Error.Conflict("Account.AlreadyConfirmed", "Email is already confirmed.");

        EmailConfirmedAt = confirmedAt;
        Status = AccountStatus.Active;
        Audit = Audit.WithUpdate(updatedBy, confirmedAt);
        Version++;

        return Result.Success;
    }

    public static Account Restore(
        Guid id,
        Email email,
        string passwordHash,
        AccountStatus status,
        DateTimeOffset? emailConfirmedAt,
        long version,
        AuditInfo audit)
        => new(id, email, passwordHash, status, emailConfirmedAt, version, audit);
}
