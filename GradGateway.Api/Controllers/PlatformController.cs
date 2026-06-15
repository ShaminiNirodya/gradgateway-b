using GradGateway.Business.DTOs;
using GradGateway.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GradGateway.Api.Controllers;

/// <summary>Public platform metadata (no admin prefix, no auth required).</summary>
[ApiController]
[Route("api/platform")]
public class PlatformController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ITestimonialService _testimonialService;

    public PlatformController(IAdminService adminService, ITestimonialService testimonialService)
    {
        _adminService = adminService;
        _testimonialService = testimonialService;
    }

    /// <summary>Registration and maintenance flags for anonymous clients (login/register pages).</summary>
    [HttpGet("settings")]
    [AllowAnonymous]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByHeader = "Accept-Encoding")]
    public async Task<ActionResult<PublicPlatformSettingsDto>> GetSettings()
    {
        var settings = await _adminService.GetPlatformSettingsAsync();
        return Ok(new PublicPlatformSettingsDto(
            settings.AllowRegistration,
            settings.MaintenanceMode,
            settings.UpdatedAt));
    }

    /// <summary>Published homepage testimonials (curated by admin).</summary>
    [HttpGet("testimonials")]
    [AllowAnonymous]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<ActionResult<IReadOnlyList<PublicTestimonialDto>>> GetTestimonials([FromQuery] int? limit = 6)
    {
        var items = await _testimonialService.GetPublishedAsync(limit);
        return Ok(items);
    }
}
