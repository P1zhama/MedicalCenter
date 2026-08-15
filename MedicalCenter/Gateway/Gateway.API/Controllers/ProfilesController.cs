using Gateway.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Profiles.Api.Protos;
using System.Linq;
using System.Threading.Tasks;

namespace Gateway.Api.Controllers;

[ApiController]
[Route("api/profiles")]
public class ProfilesController : ControllerBase
{
    private readonly ProfilesService.ProfilesServiceClient _profilesClient;

    public ProfilesController(ProfilesService.ProfilesServiceClient profilesClient)
    {
        _profilesClient = profilesClient;
    }

    [HttpPost("patients/me")]
    public async Task<IActionResult> CreateMyPatientProfile([FromBody] CreatePatientProfileWebRequest request)
    {
        var response = await _profilesClient.CreatePatientProfileAsync(new CreatePatientRequest
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName ?? string.Empty,
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = request.DateOfBirth,
            PhotoUrl = request.PhotoUrl ?? string.Empty
        });

        return Ok(ToWebResponse(response));
    }

    [HttpPost("patients/me/link")]
    public async Task<IActionResult> LinkMyPatientProfile([FromBody] LinkExistingPatientWebRequest request)
    {
        await _profilesClient.LinkExistingPatientProfileAsync(new LinkExistingPatientRequest
        {
            PatientId = request.PatientId
        });

        return Ok();
    }

    [HttpPost("patients/me/force")]
    public async Task<IActionResult> ForceCreateMyPatientProfile([FromBody] CreatePatientProfileWebRequest request)
    {
        var response = await _profilesClient.ForceCreatePatientProfileAsync(new ForceCreatePatientRequest
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName ?? string.Empty,
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = request.DateOfBirth,
            PhotoUrl = request.PhotoUrl ?? string.Empty
        });

        return Ok(ToWebResponse(response));
    }

    [HttpPost("patients/by-receptionist")]
    public async Task<IActionResult> CreatePatientByReceptionist([FromBody] CreatePatientByReceptionistWebRequest request)
    {
        var response = await _profilesClient.CreatePatientProfileByReceptionistAsync(new CreatePatientByReceptionistRequest
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName ?? string.Empty,
            DateOfBirth = request.DateOfBirth
        });

        return Ok(new CreatedProfileWebResponse(response.ProfileId));
    }

    [HttpGet("patients")]
    public async Task<IActionResult> GetPatients([FromQuery] string? search)
    {
        var response = await _profilesClient.GetPatientsAsync(new GetPatientsRequest
        {
            Search = search ?? string.Empty
        });

        var patients = response.Patients
            .Select(patient => new PatientListItemWebResponse(
                patient.PatientId,
                patient.FirstName,
                patient.LastName,
                patient.MiddleName,
                patient.PhoneNumber))
            .ToList();

        return Ok(patients);
    }

    [HttpGet("patients/me")]
    public async Task<IActionResult> GetMyPatientProfile()
    {
        var response = await _profilesClient.GetMyPatientProfileAsync(new GetMyPatientProfileRequest());

        return Ok(ToPatientWebResponse(response));
    }

    [HttpGet("patients/{id}")]
    public async Task<IActionResult> GetPatientById(string id)
    {
        var response = await _profilesClient.GetPatientByIdAsync(new GetPatientByIdRequest { PatientId = id });

        return Ok(ToPatientWebResponse(response));
    }

    [HttpPut("patients/me")]
    public async Task<IActionResult> UpdateMyPatientProfile([FromBody] UpdatePatientWebRequest request)
    {
        await _profilesClient.UpdateMyPatientProfileAsync(new UpdateMyPatientProfileRequest
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName ?? string.Empty,
            PhoneNumber = request.PhoneNumber ?? string.Empty,
            DateOfBirth = request.DateOfBirth,
            PhotoUrl = request.PhotoUrl ?? string.Empty
        });

        return Ok();
    }

    [HttpPut("patients/{id}")]
    public async Task<IActionResult> UpdatePatient(string id, [FromBody] UpdatePatientWebRequest request)
    {
        await _profilesClient.UpdatePatientAsync(new UpdatePatientRequest
        {
            PatientId = id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName ?? string.Empty,
            PhoneNumber = request.PhoneNumber ?? string.Empty,
            DateOfBirth = request.DateOfBirth,
            PhotoUrl = request.PhotoUrl ?? string.Empty
        });

        return Ok();
    }

    [HttpDelete("patients/{id}")]
    public async Task<IActionResult> DeletePatient(string id)
    {
        await _profilesClient.DeletePatientAsync(new DeletePatientRequest { PatientId = id });

        return NoContent();
    }

    [HttpPost("doctors")]
    public async Task<IActionResult> CreateDoctor([FromBody] CreateDoctorWebRequest request)
    {
        var response = await _profilesClient.CreateDoctorProfileAsync(new CreateDoctorRequest
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName ?? string.Empty,
            DateOfBirth = request.DateOfBirth,
            Email = request.Email,
            SpecializationId = request.SpecializationId,
            OfficeId = request.OfficeId,
            CareerStartYear = request.CareerStartYear,
            Status = request.Status ?? string.Empty,
            PhotoUrl = request.PhotoUrl ?? string.Empty
        });

        return Ok(new CreatedProfileWebResponse(response.DoctorId));
    }

    [HttpGet("doctors/cards")]
    public async Task<IActionResult> GetDoctorCards(
        [FromQuery] string? search,
        [FromQuery] string? specializationId,
        [FromQuery] string? officeId)
    {
        var response = await _profilesClient.GetDoctorCardsAsync(new GetDoctorCardsRequest
        {
            Search = search ?? string.Empty,
            SpecializationId = specializationId ?? string.Empty,
            OfficeId = officeId ?? string.Empty
        });

        var doctors = response.Doctors.Select(ToDoctorCardWebResponse).ToList();

        return Ok(doctors);
    }

    [HttpGet("doctors/cards/{id}")]
    public async Task<IActionResult> GetDoctorCardById(string id)
    {
        var response = await _profilesClient.GetDoctorCardByIdAsync(new GetDoctorCardByIdRequest { DoctorId = id });

        return Ok(ToDoctorCardWebResponse(response.Doctor));
    }

    [HttpGet("doctors")]
    public async Task<IActionResult> GetDoctors(
        [FromQuery] string? search,
        [FromQuery] string? specializationId,
        [FromQuery] string? officeId)
    {
        var response = await _profilesClient.GetDoctorsAsync(new GetDoctorsRequest
        {
            Search = search ?? string.Empty,
            SpecializationId = specializationId ?? string.Empty,
            OfficeId = officeId ?? string.Empty
        });

        var doctors = response.Doctors
            .Select(doctor => new DoctorListItemWebResponse(
                doctor.DoctorId,
                doctor.FirstName,
                doctor.LastName,
                doctor.MiddleName,
                doctor.DateOfBirth,
                doctor.SpecializationId,
                doctor.OfficeId,
                doctor.Status))
            .ToList();

        return Ok(doctors);
    }

    [HttpGet("doctors/me")]
    public async Task<IActionResult> GetMyDoctorProfile()
    {
        var response = await _profilesClient.GetMyDoctorProfileAsync(new GetMyDoctorProfileRequest());

        return Ok(ToDoctorWebResponse(response));
    }

    [HttpGet("doctors/{id}")]
    public async Task<IActionResult> GetDoctorById(string id)
    {
        var response = await _profilesClient.GetDoctorByIdAsync(new GetDoctorByIdRequest { DoctorId = id });

        return Ok(ToDoctorWebResponse(response));
    }

    [HttpPut("doctors/me")]
    public async Task<IActionResult> UpdateMyDoctorProfile([FromBody] UpdateMyDoctorProfileWebRequest request)
    {
        await _profilesClient.UpdateMyDoctorProfileAsync(new UpdateMyDoctorProfileRequest
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName ?? string.Empty,
            DateOfBirth = request.DateOfBirth,
            SpecializationId = request.SpecializationId,
            OfficeId = request.OfficeId,
            CareerStartYear = request.CareerStartYear,
            PhotoUrl = request.PhotoUrl ?? string.Empty
        });

        return Ok();
    }

    [HttpPut("doctors/{id}")]
    public async Task<IActionResult> UpdateDoctor(string id, [FromBody] UpdateDoctorWebRequest request)
    {
        await _profilesClient.UpdateDoctorAsync(new UpdateDoctorRequest
        {
            DoctorId = id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName ?? string.Empty,
            DateOfBirth = request.DateOfBirth,
            SpecializationId = request.SpecializationId,
            OfficeId = request.OfficeId,
            CareerStartYear = request.CareerStartYear,
            Status = request.Status,
            PhotoUrl = request.PhotoUrl ?? string.Empty
        });

        return Ok();
    }

    [HttpPatch("doctors/{id}/status")]
    public async Task<IActionResult> ChangeDoctorStatus(string id, [FromBody] ChangeStatusWebRequest request)
    {
        await _profilesClient.ChangeDoctorStatusAsync(new ChangeDoctorStatusRequest
        {
            DoctorId = id,
            Status = request.Status
        });

        return Ok();
    }

    [HttpPost("receptionists")]
    public async Task<IActionResult> CreateReceptionist([FromBody] CreateReceptionistWebRequest request)
    {
        var response = await _profilesClient.CreateReceptionistProfileAsync(new CreateReceptionistRequest
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName ?? string.Empty,
            Email = request.Email,
            OfficeId = request.OfficeId,
            PhotoUrl = request.PhotoUrl ?? string.Empty
        });

        return Ok(new CreatedProfileWebResponse(response.ReceptionistId));
    }

    [HttpGet("receptionists")]
    public async Task<IActionResult> GetReceptionists()
    {
        var response = await _profilesClient.GetReceptionistsAsync(new GetReceptionistsRequest());

        var receptionists = response.Receptionists
            .Select(receptionist => new ReceptionistListItemWebResponse(
                receptionist.ReceptionistId,
                receptionist.FirstName,
                receptionist.LastName,
                receptionist.MiddleName,
                receptionist.OfficeId,
                receptionist.Status))
            .ToList();

        return Ok(receptionists);
    }

    [HttpGet("receptionists/me")]
    public async Task<IActionResult> GetMyReceptionistProfile()
    {
        var response = await _profilesClient.GetMyReceptionistProfileAsync(new GetMyReceptionistProfileRequest());

        return Ok(ToReceptionistWebResponse(response));
    }

    [HttpGet("receptionists/{id}")]
    public async Task<IActionResult> GetReceptionistById(string id)
    {
        var response = await _profilesClient.GetReceptionistByIdAsync(
            new GetReceptionistByIdRequest { ReceptionistId = id });

        return Ok(ToReceptionistWebResponse(response));
    }

    [HttpPut("receptionists/me")]
    public async Task<IActionResult> UpdateMyReceptionistProfile(
        [FromBody] UpdateMyReceptionistProfileWebRequest request)
    {
        await _profilesClient.UpdateMyReceptionistProfileAsync(new UpdateMyReceptionistProfileRequest
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName ?? string.Empty,
            OfficeId = request.OfficeId,
            PhotoUrl = request.PhotoUrl ?? string.Empty
        });

        return Ok();
    }

    [HttpPut("receptionists/{id}")]
    public async Task<IActionResult> UpdateReceptionist(string id, [FromBody] UpdateReceptionistWebRequest request)
    {
        await _profilesClient.UpdateReceptionistAsync(new UpdateReceptionistRequest
        {
            ReceptionistId = id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName ?? string.Empty,
            OfficeId = request.OfficeId,
            Status = request.Status,
            PhotoUrl = request.PhotoUrl ?? string.Empty
        });

        return Ok();
    }

    [HttpDelete("receptionists/{id}")]
    public async Task<IActionResult> DeleteReceptionist(string id)
    {
        await _profilesClient.DeleteReceptionistAsync(new DeleteReceptionistRequest { ReceptionistId = id });

        return NoContent();
    }

    private static PatientProfileWebResponse ToWebResponse(CreatePatientResponse response)
    {
        MatchedProfileWebDto? matched = null;

        if (response.IsMatched && response.MatchedProfile != null)
        {
            matched = new MatchedProfileWebDto(
                response.MatchedProfile.ProfileId,
                response.MatchedProfile.FirstName,
                response.MatchedProfile.LastName,
                response.MatchedProfile.MiddleName,
                response.MatchedProfile.DateOfBirth);
        }

        return new PatientProfileWebResponse(response.ProfileId, response.IsMatched, matched);
    }

    private static PatientWebResponse ToPatientWebResponse(PatientResponse response)
        => new(
            response.PatientId,
            response.PhotoUrl,
            response.FirstName,
            response.LastName,
            response.MiddleName,
            response.PhoneNumber,
            response.DateOfBirth,
            response.IsLinkedToAccount);

    private static DoctorCardWebResponse ToDoctorCardWebResponse(DoctorCard doctor)
        => new(
            doctor.DoctorId,
            doctor.PhotoUrl,
            doctor.FirstName,
            doctor.LastName,
            doctor.MiddleName,
            doctor.SpecializationId,
            doctor.OfficeId,
            doctor.ExperienceYears);

    private static DoctorWebResponse ToDoctorWebResponse(DoctorResponse response)
        => new(
            response.DoctorId,
            response.PhotoUrl,
            response.FirstName,
            response.LastName,
            response.MiddleName,
            response.DateOfBirth,
            response.SpecializationId,
            response.OfficeId,
            response.CareerStartYear,
            response.ExperienceYears,
            response.Status);

    private static ReceptionistWebResponse ToReceptionistWebResponse(ReceptionistResponse response)
        => new(
            response.ReceptionistId,
            response.PhotoUrl,
            response.FirstName,
            response.LastName,
            response.MiddleName,
            response.OfficeId,
            response.Status);
}
