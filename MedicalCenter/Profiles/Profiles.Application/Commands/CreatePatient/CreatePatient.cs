using MediatR;
using System;

namespace Profiles.Application.Commands;

public record MatchedProfileDto(
    string FirstName,
    string LastName,
    string? MiddleName,
    DateOnly DateOfBirth);

public record ProfileCreationResult(
    bool IsMatchFound,
    Guid? MatchedProfileId,
    MatchedProfileDto? MatchedProfileInfo,
    Guid? CreatedProfileId);

public record CreatePatientProfileCommand(
    Guid AccountId,
    string FirstName,
    string LastName,
    string? MiddleName,
    string PhoneNumber,
    DateOnly DateOfBirth,
    string? PhotoUrl
) : IRequest<ProfileCreationResult>;