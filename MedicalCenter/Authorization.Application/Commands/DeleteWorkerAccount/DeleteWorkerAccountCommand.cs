using MediatR;
using System;

namespace Authorization.Application.Commands.DeleteWorkerAccount;

// Компенсирующая команда: удаляет учётную запись сотрудника, для которой в Profiles
// не удалось создать профиль. Возвращает false, если аккаунт уже отсутствует (идемпотентность)
public record DeleteWorkerAccountCommand(Guid AccountId) : IRequest<bool>;
