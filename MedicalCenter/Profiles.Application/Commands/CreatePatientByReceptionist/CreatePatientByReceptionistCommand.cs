using MediatR;
using System;

namespace Profiles.Application.Commands.CreatePatientByReceptionist;

// US-47: регистратор заводит пациента офлайн — без учётной записи, без номера телефона
// (этих полей просто нет в форме создания) и без проверки на совпадения,
// в отличие от самостоятельной регистрации пациента (см. CreatePatientProfileCommand)
public record CreatePatientByReceptionistCommand(
    string FirstName,
    string LastName,
    string? MiddleName,
    DateOnly DateOfBirth
) : IRequest<Guid>;
