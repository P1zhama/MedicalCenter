using Grpc.Core;
using MediatR;
using Profiles.API.ErrorMapping;
using Profiles.API.Protos;
using Profiles.Application.Commands;
using Profiles.Application.Commands.CreateDoctor;
using Profiles.Application.Commands.CreatePatientByReceptionist;
using Profiles.Application.Commands.CreateReceptionist;
using Profiles.Application.Commands.ForceCreatePatient;
using Profiles.Application.Commands.LinkExistingPatient;
using Profiles.Domain.Enums;
using System.Globalization;

namespace Profiles.API.Services;

public class ProfilesGrpcService : ProfilesService.ProfilesServiceBase
{
    private const string DateFormat = "yyyy-MM-dd";

    private readonly ISender _sender;

    public ProfilesGrpcService(ISender sender)
    {
        _sender = sender;
    }

    public override async Task<CreatePatientResponse> CreatePatientProfile(
        CreatePatientRequest request, 
        ServerCallContext context)
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

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return ToResponse(result.Value);
    }

    public override async Task<LinkExistingPatientResponse> LinkExistingPatientProfile(
        LinkExistingPatientRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.AccountId, out var accountId) ||
            !Guid.TryParse(request.PatientId, out var patientId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid AccountId or PatientId format."));
        }

        var command = new LinkExistingPatientCommand(accountId, patientId);

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new LinkExistingPatientResponse();
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

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new CreatePatientResponse
        {
            ProfileId = result.Value.ToString(),
            IsMatched = false
        };
    }

    public override async Task<CreatePatientResponse> CreatePatientProfileByReceptionist(CreatePatientByReceptionistRequest request, ServerCallContext context)
    {
        var command = new CreatePatientByReceptionistCommand(
            request.FirstName,
            request.LastName,
            NullIfEmpty(request.MiddleName),
            ParseDate(request.DateOfBirth));

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new CreatePatientResponse
        {
            ProfileId = result.Value.ToString(),
            IsMatched = false
        };
    }

    public override async Task<CreateDoctorResponse> CreateDoctorProfile(CreateDoctorRequest request, ServerCallContext context)
    {
        var status = string.IsNullOrEmpty(request.Status)
            ? DoctorStatus.AtWork
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

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new CreateDoctorResponse
        {
            DoctorId = result.Value.ToString()
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

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new CreateReceptionistResponse
        {
            ReceptionistId = result.Value.ToString()
        };
    }

    private static CreatePatientResponse ToResponse(ProfileCreationResult result)
    {
        var response = new CreatePatientResponse
        {
            IsMatched = result.IsMatchFound,
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
