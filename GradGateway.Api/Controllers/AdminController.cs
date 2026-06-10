using GradGateway.Business.DTOs;
using GradGateway.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GradGateway.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;
    private readonly IHostEnvironment _environment;

    public AdminController(
        IAdminService adminService,
        ILogger<AdminController> logger,
        IHostEnvironment environment)
    {
        _adminService = adminService;
        _logger = logger;
        _environment = environment;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        try
        {
            return Ok(await _adminService.GetDashboardAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin dashboard failed");
            var message = _environment.IsDevelopment()
                ? ex.InnerException?.Message ?? ex.Message
                : "Failed to load admin dashboard.";
            return StatusCode(500, new { message });
        }
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? role,
        [FromQuery] string? search,
        [FromQuery] bool? activeOnly)
    {
        return Ok(await _adminService.GetUsersAsync(role, search, activeOnly));
    }

    [HttpPatch("users/{userId:guid}/active")]
    public async Task<IActionResult> SetUserActive(Guid userId, [FromBody] AdminSetUserActiveDto dto)
    {
        try
        {
            await _adminService.SetUserActiveAsync(userId, dto.IsActive);
            return Ok(new { message = dto.IsActive ? "User activated." : "User suspended." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("users/{userId:guid}")]
    public async Task<IActionResult> RemoveUser(Guid userId)
    {
        try
        {
            await _adminService.RemoveUserAsync(userId);
            return Ok(new { message = "User removed from the platform." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("companies")]
    public async Task<IActionResult> GetCompanies(
        [FromQuery] string? status,
        [FromQuery] string? search)
    {
        return Ok(await _adminService.GetCompaniesAsync(status, search));
    }

    [HttpGet("inquiries")]
    public async Task<IActionResult> GetInquiries(
        [FromQuery] string? status,
        [FromQuery] string? inquiryType,
        [FromQuery] string? submitterRole)
    {
        return Ok(await _adminService.GetSupportInquiriesAsync(status, inquiryType, submitterRole));
    }

    [HttpPatch("inquiries/{inquiryId:guid}/reviewed")]
    public async Task<IActionResult> MarkInquiryReviewed(Guid inquiryId)
    {
        try
        {
            await _adminService.MarkSupportInquiryReviewedAsync(inquiryId);
            return Ok(new { message = "Inquiry marked as reviewed." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("inquiries/{inquiryId:guid}")]
    public async Task<IActionResult> DeleteInquiry(Guid inquiryId)
    {
        try
        {
            await _adminService.DeleteSupportInquiryAsync(inquiryId);
            return Ok(new { message = "Inquiry deleted." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
    {
        return Ok(await _adminService.GetPlatformSettingsAsync());
    }

    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings([FromBody] AdminUpdatePlatformSettingsDto dto)
    {
        return Ok(await _adminService.UpdatePlatformSettingsAsync(dto));
    }

    [HttpGet("settings/public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicSettings()
    {
        var settings = await _adminService.GetPlatformSettingsAsync();
        return Ok(new
        {
            settings.AllowRegistration,
            settings.MaintenanceMode
        });
    }
}
