using System;
using System.Threading;
using System.Threading.Tasks;

namespace Profiles.Application.Common.Interfaces;

public interface IAuthorizationServiceClient
{
    // Просит Authorization создать учётную запись сотрудника (с генерацией пароля и письмом)
    // и возвращает Id созданного аккаунта. roleName — "Doctor" или "Receptionist".
    Task<Guid> CreateWorkerAccountAsync(string email, string roleName, string createdBy, CancellationToken cancellationToken);

    // Компенсация: удалить аккаунт, если профиль сотрудника не удалось сохранить
    Task DeleteWorkerAccountAsync(Guid accountId, CancellationToken cancellationToken);
}
