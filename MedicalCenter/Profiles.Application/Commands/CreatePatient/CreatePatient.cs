using MediatR;
using System;

namespace Profiles.Application.Commands;

public record MatchedProfileDto(
    string FirstName,
    string LastName,
    string? MiddleName,
    DateOnly DateOfBirth);

// Результат выполнения команды
public record ProfileCreationResult(
    bool IsMatchFound,
    Guid? MatchedProfileId,
    MatchedProfileDto? MatchedProfileInfo,
    Guid? CreatedProfileId);

// Сама команда
public record CreatePatientProfileCommand(
    Guid AccountId, // Получаем из JWT токена при запросе
    string FirstName,
    string LastName,
    string? MiddleName,
    string PhoneNumber,
    DateOnly DateOfBirth,
    string? PhotoUrl
) : IRequest<ProfileCreationResult>;