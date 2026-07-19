using Authorization.Domain;
using Authorization.Domain.Enums;
using Authorization.Domain.ValueObjects;
using Authorization.Infrastructure.Persistence.Entities;
using Common.Domain;

namespace Authorization.Infrastructure.Persistence.Mappers;

public static class AccountMapper
{
    public static AccountEntity ToEntity(this Account account) => new()
    {
        Id = account.Id,
        Email = account.Email.Value,
        PasswordHash = account.PasswordHash,
        StatusId = (int)account.Status,
        EmailConfirmedAt = account.EmailConfirmedAt,
        Version = account.Version,
        CreatedBy = account.Audit.CreatedBy,
        CreatedAt = account.Audit.CreatedAt,
        UpdatedBy = account.Audit.UpdatedBy,
        UpdatedAt = account.Audit.UpdatedAt
    };

    public static Account ToDomain(this AccountEntity entity)
    {
        var emailResult = Email.Create(entity.Email);
        if (emailResult.IsError)
            throw new InvalidOperationException($"Account {entity.Id} has an invalid email stored: '{entity.Email}'.");

        return Account.Restore(
            entity.Id,
            emailResult.Value,
            entity.PasswordHash,
            (AccountStatus)entity.StatusId,
            entity.EmailConfirmedAt,
            entity.Version,
            new AuditInfo(entity.CreatedBy, entity.CreatedAt, entity.UpdatedBy, entity.UpdatedAt));
    }
}
