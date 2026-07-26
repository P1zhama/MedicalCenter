using Authorization.Domain.Constants;
using Authorization.Domain.Enums;
using Authorization.Domain.Events;
using Authorization.Domain.ValueObjects;
using Common.Domain;
using ErrorOr;

namespace Authorization.Domain;

public sealed class Account : AggregateRoot<Guid>
{
    private readonly List<AccountClaim> _claims = [];

    public Email Email { get; private set; }

    public string PasswordHash { get; private set; }

    public AccountStatus Status { get; private set; }

    public DateTimeOffset? EmailConfirmedAt { get; private set; }

    public string? EmailConfirmationTokenHash { get; private set; }

    public DateTimeOffset? EmailConfirmationTokenExpiresAt { get; private set; }

    public bool IsEmailConfirmed => EmailConfirmedAt.HasValue;

    public IReadOnlyCollection<AccountClaim> Claims => _claims.AsReadOnly();

    private Account(
        Guid id,
        Email email,
        string passwordHash,
        AccountStatus status,
        DateTimeOffset? emailConfirmedAt,
        string? emailConfirmationTokenHash,
        DateTimeOffset? emailConfirmationTokenExpiresAt,
        long version,
        AuditInfo audit)
        : base(id, version, audit)
    {
        Email = email;
        PasswordHash = passwordHash;
        Status = status;
        EmailConfirmedAt = emailConfirmedAt;
        EmailConfirmationTokenHash = emailConfirmationTokenHash;
        EmailConfirmationTokenExpiresAt = emailConfirmationTokenExpiresAt;
    }

    public static ErrorOr<Account> CreatePatient(
        Guid id,
        Email email,
        string passwordHash,
        string emailConfirmationTokenHash,
        DateTimeOffset emailConfirmationTokenExpiresAt,
        Guid createdBy,
        DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            return Error.Validation("Account.PasswordHash", "Password hash must not be empty.");

        if (string.IsNullOrWhiteSpace(emailConfirmationTokenHash))
            return Error.Validation("Account.ConfirmationTokenHash", "Confirmation token hash must not be empty.");

        var account = new Account(
            id,
            email,
            passwordHash,
            AccountStatus.PendingConfirmation,
            emailConfirmedAt: null,
            emailConfirmationTokenHash,
            emailConfirmationTokenExpiresAt,
            version: 1,
            new AuditInfo(createdBy, createdAt, null, null));

        account._claims.Add(AccountClaim.Role(Guid.NewGuid(), account.Id, Roles.Patient, createdAt));

        account.AddDomainEvent(new SignUpDomainEvent(account.Id, account.Email.Value, createdAt));

        return account;
    }

    public static ErrorOr<Account> CreateWorker(
        Guid id,
        Email email,
        string passwordHash,
        string role,
        Guid createdBy,
        DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            return Error.Validation("Account.PasswordHash", "Password hash must not be empty.");

        if (!Roles.IsWorker(role))
            return Error.Validation("Account.Role", $"Role '{role}' is not a worker role.");

        var account = new Account(
            id,
            email,
            passwordHash,
            AccountStatus.Active,
            emailConfirmedAt: createdAt,
            emailConfirmationTokenHash: null,
            emailConfirmationTokenExpiresAt: null,
            version: 1,
            new AuditInfo(createdBy, createdAt, null, null));

        account._claims.Add(AccountClaim.Role(Guid.NewGuid(), account.Id, role, createdAt));

        return account;
    }

    public ErrorOr<Success> EnsureCanSignIn()
    {
        if (Status == AccountStatus.Deactivated)
            return Error.Forbidden("Account.Deactivated", "Account is deactivated.");

        if (!IsEmailConfirmed)
            return Error.Forbidden("Account.EmailNotConfirmed", "Please confirm your email before signing in.");

        return Result.Success;
    }

    public ErrorOr<Success> ConfirmEmail(string tokenHash, DateTimeOffset now, Guid updatedBy)
    {
        if (IsEmailConfirmed)
            return Error.Conflict("Account.AlreadyConfirmed", "Email is already confirmed.");

        if (EmailConfirmationTokenHash is null || EmailConfirmationTokenExpiresAt is null)
            return Error.Validation("Account.ConfirmationTokenMissing", "There is no confirmation token to confirm.");

        if (!string.Equals(EmailConfirmationTokenHash, tokenHash, StringComparison.Ordinal))
            return Error.Validation("Account.ConfirmationTokenInvalid", "Confirmation link is invalid.");

        if (now >= EmailConfirmationTokenExpiresAt.Value)
            return Error.Validation("Account.ConfirmationTokenExpired", "Confirmation link has expired.");

        EmailConfirmedAt = now;
        Status = AccountStatus.Active;
        EmailConfirmationTokenHash = null;
        EmailConfirmationTokenExpiresAt = null;
        Audit = Audit.WithUpdate(updatedBy, now);
        Version++;

        return Result.Success;
    }

    public ErrorOr<Success> ReissueConfirmation(
        string newPasswordHash,
        string emailConfirmationTokenHash,
        DateTimeOffset emailConfirmationTokenExpiresAt,
        DateTimeOffset now)
    {
        if (IsEmailConfirmed)
            return Error.Conflict("Account.AlreadyConfirmed", "Email is already confirmed.");

        if (string.IsNullOrWhiteSpace(newPasswordHash))
            return Error.Validation("Account.PasswordHash", "Password hash must not be empty.");

        if (string.IsNullOrWhiteSpace(emailConfirmationTokenHash))
            return Error.Validation("Account.ConfirmationTokenHash", "Confirmation token hash must not be empty.");

        PasswordHash = newPasswordHash;
        EmailConfirmationTokenHash = emailConfirmationTokenHash;
        EmailConfirmationTokenExpiresAt = emailConfirmationTokenExpiresAt;
        Audit = Audit.WithUpdate(Id, now);
        Version++;

        AddDomainEvent(new SignUpDomainEvent(Id, Email.Value, now));

        return Result.Success;
    }

    public ErrorOr<Success> GrantPermission(string permission, DateTimeOffset grantedAt, Guid updatedBy)
    {
        if (string.IsNullOrWhiteSpace(permission))
            return Error.Validation("Account.Permission", "Permission must not be empty.");

        if (HasClaim(AppClaimTypes.Permission, permission))
            return Error.Conflict("Account.PermissionAlreadyGranted", $"Permission '{permission}' is already granted.");

        _claims.Add(AccountClaim.Permission(Guid.NewGuid(), Id, permission, grantedAt));
        Audit = Audit.WithUpdate(updatedBy, grantedAt);
        Version++;

        return Result.Success;
    }

    public ErrorOr<Success> RevokePermission(string permission, DateTimeOffset revokedAt, Guid updatedBy)
    {
        var claim = _claims.FirstOrDefault(
            existing => existing.Type == AppClaimTypes.Permission && existing.Value == permission);

        if (claim is null)
            return Error.NotFound("Account.PermissionNotGranted", $"Permission '{permission}' is not granted.");

        _claims.Remove(claim);
        Audit = Audit.WithUpdate(updatedBy, revokedAt);
        Version++;

        return Result.Success;
    }

    public bool HasClaim(string type, string value)
        => _claims.Any(claim => claim.Type == type && claim.Value == value);

    public static Account Restore(
        Guid id,
        Email email,
        string passwordHash,
        AccountStatus status,
        DateTimeOffset? emailConfirmedAt,
        string? emailConfirmationTokenHash,
        DateTimeOffset? emailConfirmationTokenExpiresAt,
        long version,
        AuditInfo audit,
        IEnumerable<AccountClaim> claims)
    {
        var account = new Account(
            id,
            email,
            passwordHash,
            status,
            emailConfirmedAt,
            emailConfirmationTokenHash,
            emailConfirmationTokenExpiresAt,
            version,
            audit);

        account._claims.AddRange(claims);

        return account;
    }
}
