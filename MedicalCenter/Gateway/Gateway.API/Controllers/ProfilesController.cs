using Gateway.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Profiles.Api.Protos;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Gateway.Api.Controllers;

[ApiController]
[Route("api/profiles")]
[Authorize]
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
            AccountId = GetAccountId(),
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
            AccountId = GetAccountId(),
            PatientId = request.PatientId
        });

        return Ok();
    }

    [HttpPost("patients/me/force")]
    public async Task<IActionResult> ForceCreateMyPatientProfile([FromBody] CreatePatientProfileWebRequest request)
    {
        var response = await _profilesClient.ForceCreatePatientProfileAsync(new ForceCreatePatientRequest
        {
            AccountId = GetAccountId(),
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
            PhotoUrl = request.PhotoUrl ?? string.Empty,
            CreatedBy = GetEmail()
        });

        return Ok(new CreatedProfileWebResponse(response.DoctorId));
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
            PhotoUrl = request.PhotoUrl ?? string.Empty,
            CreatedBy = GetEmail()
        });

        return Ok(new CreatedProfileWebResponse(response.ReceptionistId));
    }

    private string GetAccountId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Токен не содержит идентификатор аккаунта.");

    private string GetEmail() =>
        User.FindFirstValue(ClaimTypes.Email)
        ?? throw new InvalidOperationException("Токен не содержит email.");

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
}
