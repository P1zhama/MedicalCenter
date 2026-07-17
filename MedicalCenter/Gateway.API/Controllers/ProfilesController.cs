using Gateway.API.Models;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Profiles.API.Protos;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Gateway.API.Controllers;

[ApiController]
[Route("api/profiles")]
[Authorize]
public class ProfilesController : ControllerBase
{
    private readonly ProfilesService.ProfilesServiceClient _profilesClient;
    private readonly ILogger<ProfilesController> _logger;

    public ProfilesController(ProfilesService.ProfilesServiceClient profilesClient, ILogger<ProfilesController> logger)
    {
        _profilesClient = profilesClient;
        _logger = logger;
    }

    [HttpPost("patients/me")]
    public async Task<IActionResult> CreateMyPatientProfile([FromBody] CreatePatientProfileWebRequest request)
    {
        var grpcRequest = new CreatePatientRequest
        {
            AccountId = GetAccountId(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName ?? string.Empty,
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = request.DateOfBirth,
            PhotoUrl = request.PhotoUrl ?? string.Empty
        };

        try
        {
            var response = await _profilesClient.CreatePatientProfileAsync(grpcRequest);
            return Ok(ToWebResponse(response));
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Error creating patient profile");
            return BadRequest(new { error = ex.Status.Detail });
        }
    }

    [HttpPost("patients/me/link")]
    public async Task<IActionResult> LinkMyPatientProfile([FromBody] LinkExistingPatientWebRequest request)
    {
        var grpcRequest = new LinkExistingPatientRequest
        {
            AccountId = GetAccountId(),
            PatientId = request.PatientId
        };

        try
        {
            var response = await _profilesClient.LinkExistingPatientProfileAsync(grpcRequest);
            return Ok(new LinkExistingPatientWebResponse(response.Success, response.Message));
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Error linking patient profile");
            return BadRequest(new { error = ex.Status.Detail });
        }
    }

    [HttpPost("patients/me/force")]
    public async Task<IActionResult> ForceCreateMyPatientProfile([FromBody] CreatePatientProfileWebRequest request)
    {
        var grpcRequest = new ForceCreatePatientRequest
        {
            AccountId = GetAccountId(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName ?? string.Empty,
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = request.DateOfBirth,
            PhotoUrl = request.PhotoUrl ?? string.Empty
        };

        try
        {
            var response = await _profilesClient.ForceCreatePatientProfileAsync(grpcRequest);
            return Ok(ToWebResponse(response));
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Error force-creating patient profile");
            return BadRequest(new { error = ex.Status.Detail });
        }
    }

    [HttpPost("patients/by-receptionist")]
    [Authorize(Roles = "Receptionist")]
    public async Task<IActionResult> CreatePatientByReceptionist([FromBody] CreatePatientByReceptionistWebRequest request)
    {
        var grpcRequest = new CreatePatientByReceptionistRequest
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName ?? string.Empty,
            DateOfBirth = request.DateOfBirth
        };

        try
        {
            var response = await _profilesClient.CreatePatientProfileByReceptionistAsync(grpcRequest);
            return Ok(new CreatedProfileWebResponse(response.ProfileId, response.Message));
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Error creating patient profile by receptionist");
            return BadRequest(new { error = ex.Status.Detail });
        }
    }

    [HttpPost("doctors")]
    [Authorize(Roles = "Receptionist")]
    public async Task<IActionResult> CreateDoctor([FromBody] CreateDoctorWebRequest request)
    {
        var grpcRequest = new CreateDoctorRequest
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
        };

        try
        {
            var response = await _profilesClient.CreateDoctorProfileAsync(grpcRequest);
            return Ok(new CreatedProfileWebResponse(response.DoctorId, response.Message));
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Error creating doctor profile");
            return BadRequest(new { error = ex.Status.Detail });
        }
    }

    [HttpPost("receptionists")]
    [Authorize(Roles = "Receptionist")]
    public async Task<IActionResult> CreateReceptionist([FromBody] CreateReceptionistWebRequest request)
    {
        var grpcRequest = new CreateReceptionistRequest
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName ?? string.Empty,
            Email = request.Email,
            OfficeId = request.OfficeId,
            PhotoUrl = request.PhotoUrl ?? string.Empty,
            CreatedBy = GetEmail()
        };

        try
        {
            var response = await _profilesClient.CreateReceptionistProfileAsync(grpcRequest);
            return Ok(new CreatedProfileWebResponse(response.ReceptionistId, response.Message));
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Error creating receptionist profile");
            return BadRequest(new { error = ex.Status.Detail });
        }
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

        return new PatientProfileWebResponse(response.ProfileId, response.IsMatched, response.Message, matched);
    }
}
