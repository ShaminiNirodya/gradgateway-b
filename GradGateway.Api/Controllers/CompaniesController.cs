using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GradGateway.Business.DTOs;
using GradGateway.Business.Interfaces;

namespace GradGateway.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companyService;
    private readonly ILogger<CompaniesController> _logger;

    public CompaniesController(ICompanyService companyService, ILogger<CompaniesController> logger)
    {
        _companyService = companyService;
        _logger = logger;
    }

    [HttpPost("register")]
    [Authorize]
    public async Task<IActionResult> RegisterCompany([FromBody] CompanyRegistrationDto dto)
    {
        try
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("user_id")?.Value;

            if (string.IsNullOrWhiteSpace(firebaseUid))
            {
                return Unauthorized(new { message = "Invalid token: Firebase UID not found" });
            }

            if (!string.Equals(firebaseUid, dto.FirebaseUid, StringComparison.Ordinal))
            {
                return BadRequest(new { message = "Firebase UID mismatch" });
            }

            var result = await _companyService.RegisterOrUpdateCompanyAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering company");
            return StatusCode(500, new { message = "An error occurred while registering company" });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentCompany()
    {
        try
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("user_id")?.Value;

            if (string.IsNullOrWhiteSpace(firebaseUid))
            {
                return Unauthorized(new { message = "Invalid token: Firebase UID not found" });
            }

            var company = await _companyService.GetCompanyByFirebaseUidAsync(firebaseUid);
            if (company == null)
            {
                return NotFound(new { message = "Company profile not found" });
            }

            return Ok(company);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting company profile");
            return StatusCode(500, new { message = "An error occurred while retrieving company profile" });
        }
    }
}
