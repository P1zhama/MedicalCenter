using Grpc.Core;
using MediatR;
using Profiles.Api.ErrorMapping;
using Profiles.Api.Protos;
using Profiles.Application.Commands;
using Profiles.Application.Commands.ChangeDoctorStatus;
using Profiles.Application.Commands.CreateDoctor;
using Profiles.Application.Commands.CreatePatientByReceptionist;
using Profiles.Application.Commands.CreateReceptionist;
using Profiles.Application.Commands.DeletePatient;
using Profiles.Application.Commands.DeleteReceptionist;
using Profiles.Application.Commands.ForceCreatePatient;
using Profiles.Application.Commands.LinkExistingPatient;
using Profiles.Application.Commands.UpdateDoctor;
using Profiles.Application.Commands.UpdateMyDoctorProfile;
using Profiles.Application.Commands.UpdateMyPatientProfile;
using Profiles.Application.Commands.UpdateMyReceptionistProfile;
using Profiles.Application.Commands.UpdatePatient;
using Profiles.Application.Commands.UpdateReceptionist;
using Profiles.Application.Common.Dtos;
using Profiles.Application.Queries.GetDoctorById;
using Profiles.Application.Queries.GetDoctorCardById;
using Profiles.Application.Queries.GetDoctorCards;
using Profiles.Application.Queries.GetDoctors;
using Profiles.Application.Queries.GetMyDoctorProfile;
using Profiles.Application.Queries.GetMyPatientProfile;
using Profiles.Application.Queries.GetMyReceptionistProfile;
using Profiles.Application.Queries.GetPatientById;
using Profiles.Application.Queries.GetPatients;
using Profiles.Application.Queries.GetReceptionistById;
using Profiles.Application.Queries.GetReceptionists;
using Profiles.Domain.Enums;
using System.Globalization;

namespace Profiles.Api.Services;

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
            ParseGuid(request.AccountId, "account id"),
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
        var command = new LinkExistingPatientCommand(
            ParseGuid(request.AccountId, "account id"),
            ParseGuid(request.PatientId, "patient id"));

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new LinkExistingPatientResponse();
    }

    public override async Task<CreatePatientResponse> ForceCreatePatientProfile(
        ForceCreatePatientRequest request,
        ServerCallContext context)
    {
        var command = new ForceCreatePatientCommand(
            ParseGuid(request.AccountId, "account id"),
            request.FirstName,
            request.LastName,
            NullIfEmpty(request.MiddleName),
            request.PhoneNumber,
            ParseDate(request.DateOfBirth),
            NullIfEmpty(request.PhotoUrl));

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new CreatePatientResponse { ProfileId = result.Value.ToString(), IsMatched = false };
    }

    public override async Task<CreatePatientResponse> CreatePatientProfileByReceptionist(
        CreatePatientByReceptionistRequest request,
        ServerCallContext context)
    {
        var command = new CreatePatientByReceptionistCommand(
            request.FirstName,
            request.LastName,
            NullIfEmpty(request.MiddleName),
            ParseDate(request.DateOfBirth));

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new CreatePatientResponse { ProfileId = result.Value.ToString(), IsMatched = false };
    }

    public override async Task<CreateDoctorResponse> CreateDoctorProfile(
        CreateDoctorRequest request,
        ServerCallContext context)
    {
        var command = new CreateDoctorCommand(
            request.FirstName,
            request.LastName,
            NullIfEmpty(request.MiddleName),
            ParseDate(request.DateOfBirth),
            request.Email,
            ParseGuid(request.SpecializationId, "specialization id"),
            ParseGuid(request.OfficeId, "office id"),
            request.CareerStartYear,
            ParseDoctorStatus(request.Status),
            NullIfEmpty(request.PhotoUrl));

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new CreateDoctorResponse { DoctorId = result.Value.ToString() };
    }

    public override async Task<CreateReceptionistResponse> CreateReceptionistProfile(
        CreateReceptionistRequest request,
        ServerCallContext context)
    {
        var command = new CreateReceptionistCommand(
            request.FirstName,
            request.LastName,
            NullIfEmpty(request.MiddleName),
            request.Email,
            ParseGuid(request.OfficeId, "office id"),
            NullIfEmpty(request.PhotoUrl));

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new CreateReceptionistResponse { ReceptionistId = result.Value.ToString() };
    }

    public override async Task<GetPatientsResponse> GetPatients(
        GetPatientsRequest request,
        ServerCallContext context)
    {
        var result = await _sender.Send(new GetPatientsQuery(NullIfEmpty(request.Search)), context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        var response = new GetPatientsResponse();

        foreach (var patient in result.Value)
        {
            response.Patients.Add(new PatientListItem
            {
                PatientId = patient.Id.ToString(),
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                MiddleName = patient.MiddleName ?? string.Empty,
                PhoneNumber = patient.PhoneNumber ?? string.Empty
            });
        }

        return response;
    }

    public override async Task<PatientResponse> GetPatientById(
        GetPatientByIdRequest request,
        ServerCallContext context)
    {
        var query = new GetPatientByIdQuery(ParseGuid(request.PatientId, "patient id"));

        var result = await _sender.Send(query, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return ToPatientResponse(result.Value);
    }

    public override async Task<PatientResponse> GetMyPatientProfile(
        GetMyPatientProfileRequest request,
        ServerCallContext context)
    {
        var result = await _sender.Send(new GetMyPatientProfileQuery(), context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return ToPatientResponse(result.Value);
    }

    public override async Task<UpdatePatientResponse> UpdatePatient(
        UpdatePatientRequest request,
        ServerCallContext context)
    {
        var command = new UpdatePatientCommand(
            ParseGuid(request.PatientId, "patient id"),
            request.FirstName,
            request.LastName,
            NullIfEmpty(request.MiddleName),
            NullIfEmpty(request.PhoneNumber),
            ParseDate(request.DateOfBirth),
            NullIfEmpty(request.PhotoUrl));

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new UpdatePatientResponse();
    }

    public override async Task<UpdatePatientResponse> UpdateMyPatientProfile(
        UpdateMyPatientProfileRequest request,
        ServerCallContext context)
    {
        var command = new UpdateMyPatientProfileCommand(
            request.FirstName,
            request.LastName,
            NullIfEmpty(request.MiddleName),
            NullIfEmpty(request.PhoneNumber),
            ParseDate(request.DateOfBirth),
            NullIfEmpty(request.PhotoUrl));

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new UpdatePatientResponse();
    }

    public override async Task<DeletePatientResponse> DeletePatient(
        DeletePatientRequest request,
        ServerCallContext context)
    {
        var command = new DeletePatientCommand(ParseGuid(request.PatientId, "patient id"));

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new DeletePatientResponse();
    }

    public override async Task<GetDoctorCardsResponse> GetDoctorCards(
        GetDoctorCardsRequest request,
        ServerCallContext context)
    {
        var query = new GetDoctorCardsQuery(
            NullIfEmpty(request.Search),
            ParseNullableGuid(request.SpecializationId, "specialization id"),
            ParseNullableGuid(request.OfficeId, "office id"));

        var result = await _sender.Send(query, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        var response = new GetDoctorCardsResponse();

        foreach (var doctor in result.Value)
        {
            response.Doctors.Add(ToDoctorCard(doctor));
        }

        return response;
    }

    public override async Task<DoctorCardResponse> GetDoctorCardById(
        GetDoctorCardByIdRequest request,
        ServerCallContext context)
    {
        var query = new GetDoctorCardByIdQuery(ParseGuid(request.DoctorId, "doctor id"));

        var result = await _sender.Send(query, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new DoctorCardResponse { Doctor = ToDoctorCard(result.Value) };
    }

    public override async Task<GetDoctorsResponse> GetDoctors(
        GetDoctorsRequest request,
        ServerCallContext context)
    {
        var query = new GetDoctorsQuery(
            NullIfEmpty(request.Search),
            ParseNullableGuid(request.SpecializationId, "specialization id"),
            ParseNullableGuid(request.OfficeId, "office id"));

        var result = await _sender.Send(query, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        var response = new GetDoctorsResponse();

        foreach (var doctor in result.Value)
        {
            response.Doctors.Add(new DoctorListItem
            {
                DoctorId = doctor.Id.ToString(),
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                MiddleName = doctor.MiddleName ?? string.Empty,
                DateOfBirth = FormatDate(doctor.DateOfBirth),
                SpecializationId = doctor.SpecializationId.ToString(),
                OfficeId = doctor.OfficeId.ToString(),
                Status = doctor.Status
            });
        }

        return response;
    }

    public override async Task<DoctorResponse> GetDoctorById(
        GetDoctorByIdRequest request,
        ServerCallContext context)
    {
        var query = new GetDoctorByIdQuery(ParseGuid(request.DoctorId, "doctor id"));

        var result = await _sender.Send(query, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return ToDoctorResponse(result.Value);
    }

    public override async Task<DoctorResponse> GetMyDoctorProfile(
        GetMyDoctorProfileRequest request,
        ServerCallContext context)
    {
        var result = await _sender.Send(new GetMyDoctorProfileQuery(), context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return ToDoctorResponse(result.Value);
    }

    public override async Task<UpdateDoctorResponse> UpdateDoctor(
        UpdateDoctorRequest request,
        ServerCallContext context)
    {
        var command = new UpdateDoctorCommand(
            ParseGuid(request.DoctorId, "doctor id"),
            request.FirstName,
            request.LastName,
            NullIfEmpty(request.MiddleName),
            ParseDate(request.DateOfBirth),
            ParseGuid(request.SpecializationId, "specialization id"),
            ParseGuid(request.OfficeId, "office id"),
            request.CareerStartYear,
            ParseDoctorStatus(request.Status),
            NullIfEmpty(request.PhotoUrl));

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new UpdateDoctorResponse();
    }

    public override async Task<UpdateDoctorResponse> UpdateMyDoctorProfile(
        UpdateMyDoctorProfileRequest request,
        ServerCallContext context)
    {
        var command = new UpdateMyDoctorProfileCommand(
            request.FirstName,
            request.LastName,
            NullIfEmpty(request.MiddleName),
            ParseDate(request.DateOfBirth),
            ParseGuid(request.SpecializationId, "specialization id"),
            ParseGuid(request.OfficeId, "office id"),
            request.CareerStartYear,
            NullIfEmpty(request.PhotoUrl));

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new UpdateDoctorResponse();
    }

    public override async Task<ChangeDoctorStatusResponse> ChangeDoctorStatus(
        ChangeDoctorStatusRequest request,
        ServerCallContext context)
    {
        var command = new ChangeDoctorStatusCommand(
            ParseGuid(request.DoctorId, "doctor id"),
            ParseDoctorStatus(request.Status));

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new ChangeDoctorStatusResponse();
    }

    public override async Task<GetReceptionistsResponse> GetReceptionists(
        GetReceptionistsRequest request,
        ServerCallContext context)
    {
        var result = await _sender.Send(new GetReceptionistsQuery(), context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        var response = new GetReceptionistsResponse();

        foreach (var receptionist in result.Value)
        {
            response.Receptionists.Add(new ReceptionistListItem
            {
                ReceptionistId = receptionist.Id.ToString(),
                FirstName = receptionist.FirstName,
                LastName = receptionist.LastName,
                MiddleName = receptionist.MiddleName ?? string.Empty,
                OfficeId = receptionist.OfficeId.ToString(),
                Status = receptionist.Status
            });
        }

        return response;
    }

    public override async Task<ReceptionistResponse> GetReceptionistById(
        GetReceptionistByIdRequest request,
        ServerCallContext context)
    {
        var query = new GetReceptionistByIdQuery(ParseGuid(request.ReceptionistId, "receptionist id"));

        var result = await _sender.Send(query, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return ToReceptionistResponse(result.Value);
    }

    public override async Task<ReceptionistResponse> GetMyReceptionistProfile(
        GetMyReceptionistProfileRequest request,
        ServerCallContext context)
    {
        var result = await _sender.Send(new GetMyReceptionistProfileQuery(), context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return ToReceptionistResponse(result.Value);
    }

    public override async Task<UpdateReceptionistResponse> UpdateReceptionist(
        UpdateReceptionistRequest request,
        ServerCallContext context)
    {
        var command = new UpdateReceptionistCommand(
            ParseGuid(request.ReceptionistId, "receptionist id"),
            request.FirstName,
            request.LastName,
            NullIfEmpty(request.MiddleName),
            ParseGuid(request.OfficeId, "office id"),
            ParseReceptionistStatus(request.Status),
            NullIfEmpty(request.PhotoUrl));

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new UpdateReceptionistResponse();
    }

    public override async Task<UpdateReceptionistResponse> UpdateMyReceptionistProfile(
        UpdateMyReceptionistProfileRequest request,
        ServerCallContext context)
    {
        var command = new UpdateMyReceptionistProfileCommand(
            request.FirstName,
            request.LastName,
            NullIfEmpty(request.MiddleName),
            ParseGuid(request.OfficeId, "office id"),
            NullIfEmpty(request.PhotoUrl));

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new UpdateReceptionistResponse();
    }

    public override async Task<DeleteReceptionistResponse> DeleteReceptionist(
        DeleteReceptionistRequest request,
        ServerCallContext context)
    {
        var command = new DeleteReceptionistCommand(ParseGuid(request.ReceptionistId, "receptionist id"));

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new DeleteReceptionistResponse();
    }

    private static DoctorCard ToDoctorCard(DoctorCardDto doctor) => new()
    {
        DoctorId = doctor.Id.ToString(),
        PhotoUrl = doctor.PhotoUrl ?? string.Empty,
        FirstName = doctor.FirstName,
        LastName = doctor.LastName,
        MiddleName = doctor.MiddleName ?? string.Empty,
        SpecializationId = doctor.SpecializationId.ToString(),
        OfficeId = doctor.OfficeId.ToString(),
        ExperienceYears = doctor.ExperienceYears
    };

    private static DoctorResponse ToDoctorResponse(DoctorDto doctor) => new()
    {
        DoctorId = doctor.Id.ToString(),
        PhotoUrl = doctor.PhotoUrl ?? string.Empty,
        FirstName = doctor.FirstName,
        LastName = doctor.LastName,
        MiddleName = doctor.MiddleName ?? string.Empty,
        DateOfBirth = FormatDate(doctor.DateOfBirth),
        SpecializationId = doctor.SpecializationId.ToString(),
        OfficeId = doctor.OfficeId.ToString(),
        CareerStartYear = doctor.CareerStartYear,
        ExperienceYears = doctor.ExperienceYears,
        Status = doctor.Status
    };

    private static PatientResponse ToPatientResponse(PatientDto patient) => new()
    {
        PatientId = patient.Id.ToString(),
        PhotoUrl = patient.PhotoUrl ?? string.Empty,
        FirstName = patient.FirstName,
        LastName = patient.LastName,
        MiddleName = patient.MiddleName ?? string.Empty,
        PhoneNumber = patient.PhoneNumber ?? string.Empty,
        DateOfBirth = FormatDate(patient.DateOfBirth),
        IsLinkedToAccount = patient.IsLinkedToAccount
    };

    private static ReceptionistResponse ToReceptionistResponse(ReceptionistDto receptionist) => new()
    {
        ReceptionistId = receptionist.Id.ToString(),
        PhotoUrl = receptionist.PhotoUrl ?? string.Empty,
        FirstName = receptionist.FirstName,
        LastName = receptionist.LastName,
        MiddleName = receptionist.MiddleName ?? string.Empty,
        OfficeId = receptionist.OfficeId.ToString(),
        Status = receptionist.Status
    };

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
                DateOfBirth = FormatDate(result.MatchedProfileInfo.DateOfBirth)
            };
        }

        return response;
    }

    private static Guid ParseGuid(string value, string fieldName)
        => Guid.TryParse(value, out var id)
            ? id
            : throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid {fieldName} format."));

    private static Guid? ParseNullableGuid(string value, string fieldName)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        return ParseGuid(value, fieldName);
    }

    private static DateOnly ParseDate(string value)
        => DateOnly.TryParseExact(value, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : throw new RpcException(new Status(StatusCode.InvalidArgument, "Please, select the date"));

    private static string FormatDate(DateOnly value) => value.ToString(DateFormat, CultureInfo.InvariantCulture);

    private static DoctorStatus ParseDoctorStatus(string status)
    {
        if (string.IsNullOrEmpty(status))
            return DoctorStatus.AtWork;

        if (!Enum.TryParse<DoctorStatus>(status, ignoreCase: true, out var parsed) || !Enum.IsDefined(parsed))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid doctor status."));

        return parsed;
    }

    private static ReceptionistStatus ParseReceptionistStatus(string status)
    {
        if (string.IsNullOrEmpty(status))
            return ReceptionistStatus.Active;

        if (!Enum.TryParse<ReceptionistStatus>(status, ignoreCase: true, out var parsed) || !Enum.IsDefined(parsed))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid receptionist status."));

        return parsed;
    }

    private static string? NullIfEmpty(string value) => string.IsNullOrEmpty(value) ? null : value;
}
