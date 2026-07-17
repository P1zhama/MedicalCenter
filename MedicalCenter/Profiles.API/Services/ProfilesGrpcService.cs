using Grpc.Core;
using MediatR;
using Profiles.API.Protos;
using Profiles.Application.Commands;
using Profiles.Application.Commands.CreateDoctor;
using Profiles.Application.Commands.CreatePatientByReceptionist;
using Profiles.Application.Commands.CreateReceptionist;
using Profiles.Application.Commands.ForceCreatePatient;
using Profiles.Application.Commands.LinkExistingPatient;
using Profiles.Domain.Enums;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Profiles.API.Services;

public class ProfilesGrpcService : ProfilesService.ProfilesServiceBase
{
    private const string DateFormat = "yyyy-MM-dd";

    private readonly ISender _sender;

    public ProfilesGrpcService(ISender sender)
    {
        _sender = sender;
    }

    public override async Task<CreatePatientResponse> CreatePatientProfile(CreatePatientRequest request, ServerCallContext context)
    {
        var command = new CreatePatientProfileCommand(
            Guid.Parse(request.AccountId),
            request.FirstName,
            request.LastName,
            NullIfEmpty(request.MiddleName),
            request.PhoneNumber,
            ParseDate(request.DateOfBirth),
            NullIfEmpty(request.PhotoUrl));

        var result = await _sender.Send(command, context.CancellationToken);

        return ToResponse(result);
    }

    public override async Task<LinkExistingPatientResponse> LinkExistingPatientProfile(LinkExistingPatientRequest request, ServerCallContext context)
    {
        var command = new LinkExistingPatientCommand(
            Guid.Parse(request.AccountId),
            Guid.Parse(request.PatientId));

        var success = await _sender.Send(command, context.CancellationToken);

        return new LinkExistingPatientResponse
        {
            Success = success,
            Message = success ? "Profile linked successfully." : "Profile is already linked or was not found."
        };
    }

    public override async Task<CreatePatientResponse> ForceCreatePatientProfile(ForceCreatePatientRequest request, ServerCallContext context)
    {
        var command = new ForceCreatePatientCommand(
            Guid.Parse(request.AccountId),
            request.FirstName,
            request.LastName,
            NullIfEmpty(request.MiddleName),
            request.PhoneNumber,
            ParseDate(request.DateOfBirth),
            NullIfEmpty(request.PhotoUrl));

        var profileId = await _sender.Send(command, context.CancellationToken);

        return new CreatePatientResponse
        {
            ProfileId = profileId.ToString(),
            IsMatched = false,
            Message = "Profile created successfully."
        };
    }

    public override async Task<CreatePatientResponse> CreatePatientProfileByReceptionist(CreatePatientByReceptionistRequest request, ServerCallContext context)
    {
        var command = new CreatePatientByReceptionistCommand(
            request.FirstName,
            request.LastName,
            NullIfEmpty(request.MiddleName),
            ParseDate(request.DateOfBirth));

        var profileId = await _sender.Send(command, context.CancellationToken);

        return new CreatePatientResponse
        {
            ProfileId = profileId.ToString(),
            IsMatched = false,
            Message = "Patient profile created successfully by receptionist."
        };
    }

    public override async Task<CreateDoctorResponse> CreateDoctorProfile(CreateDoctorRequest request, ServerCallContext context)
    {
        var status = string.IsNullOrEmpty(request.Status)
            ? DoctorStatus.AtWork // значение по умолчанию, F-10 в US-9
            : Enum.Parse<DoctorStatus>(request.Status, ignoreCase: true);

        var command = new CreateDoctorCommand(
            request.FirstName,
            request.LastName,
            NullIfEmpty(request.MiddleName),
            ParseDate(request.DateOfBirth),
            request.Email,
            Guid.Parse(request.SpecializationId),
            Guid.Parse(request.OfficeId),
            request.CareerStartYear,
            status,
            NullIfEmpty(request.PhotoUrl),
            request.CreatedBy);

        var doctorId = await _sender.Send(command, context.CancellationToken);

        return new CreateDoctorResponse
        {
            DoctorId = doctorId.ToString(),
            Message = "Doctor profile created successfully."
        };
    }

    public override async Task<CreateReceptionistResponse> CreateReceptionistProfile(CreateReceptionistRequest request, ServerCallContext context)
    {
        var command = new CreateReceptionistCommand(
            request.FirstName,
            request.LastName,
            NullIfEmpty(request.MiddleName),
            request.Email,
            Guid.Parse(request.OfficeId),
            NullIfEmpty(request.PhotoUrl),
            request.CreatedBy);

        var receptionistId = await _sender.Send(command, context.CancellationToken);

        return new CreateReceptionistResponse
        {
            ReceptionistId = receptionistId.ToString(),
            Message = "Receptionist profile created successfully."
        };
    }

    private static CreatePatientResponse ToResponse(ProfileCreationResult result)
    {
        var response = new CreatePatientResponse
        {
            IsMatched = result.IsMatchFound,
            Message = result.IsMatchFound
                ? "A similar profile has been found, you might have already visited one of our clinics?"
                : "Profile created successfully.",
            ProfileId = (result.CreatedProfileId ?? result.MatchedProfileId ?? Guid.Empty).ToString()
        };

        if (result.IsMatchFound && result.MatchedProfileInfo != null && result.MatchedProfileId.HasValue)
        {
            response.MatchedProfile = new MatchedProfile
            {
                ProfileId = result.MatchedProfileId.Value.ToString(),
                FirstName = result.MatchedProfileInfo.FirstName,
                LastName = result.MatchedProfileInfo.LastName,
                MiddleName = result.MatchedProfileInfo.MiddleName ?? string.Empty,
                DateOfBirth = result.MatchedProfileInfo.DateOfBirth.ToString(DateFormat, CultureInfo.InvariantCulture)
            };
        }

        return response;
    }

    private static DateOnly ParseDate(string value) =>
        DateOnly.ParseExact(value, DateFormat, CultureInfo.InvariantCulture);

    private static string? NullIfEmpty(string value) => string.IsNullOrEmpty(value) ? null : value;
}
