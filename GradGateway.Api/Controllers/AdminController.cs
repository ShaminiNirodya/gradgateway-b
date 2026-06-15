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
    private readonly ITestimonialService _testimonialService;
    private readonly ILogger<AdminController> _logger;
    private readonly IHostEnvironment _environment;

    public AdminController(
        IAdminService adminService,
        ITestimonialService testimonialService,
        ILogger<AdminController> logger,
        IHostEnvironment environment)
    {
        _adminService = adminService;
        _testimonialService = testimonialService;
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

    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalytics()
    {
        try
        {
            return Ok(await _adminService.GetAnalyticsAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin analytics failed");
            var message = _environment.IsDevelopment()
                ? ex.InnerException?.Message ?? ex.Message
                : "Failed to load admin analytics.";
            return StatusCode(500, new { message });
        }
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? role,
        [FromQuery] string? search,
        [FromQuery] bool? activeOnly,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize)
    {
        return Ok(await _adminService.GetUsersAsync(role, search, activeOnly, page, pageSize));
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
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize)
    {
        return Ok(await _adminService.GetCompaniesAsync(status, search, page, pageSize));
    }

    [HttpGet("inquiries")]
    public async Task<IActionResult> GetInquiries(
        [FromQuery] string? status,
        [FromQuery] string? inquiryType,
        [FromQuery] string? submitterRole,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize)
    {
        return Ok(await _adminService.GetSupportInquiriesAsync(status, inquiryType, submitterRole, page, pageSize));
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

    [HttpGet("email-logs")]
    public async Task<IActionResult> GetEmailLogs(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        return Ok(await _adminService.GetEmailLogsAsync(search, status, page, pageSize));
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

    [HttpGet("testimonials")]
    public async Task<IActionResult> GetTestimonials(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize)
    {
        return Ok(await _testimonialService.GetAdminListAsync(status, page, pageSize));
    }

    [HttpPost("testimonials")]
    public async Task<IActionResult> CreateTestimonial([FromBody] AdminCreateTestimonialDto dto)
    {
        try
        {
            return Ok(await _testimonialService.CreateAdminAsync(dto));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("testimonials/{testimonialId:guid}")]
    public async Task<IActionResult> UpdateTestimonial(Guid testimonialId, [FromBody] AdminUpdateTestimonialDto dto)
    {
        try
        {
            return Ok(await _testimonialService.UpdateAsync(testimonialId, dto));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("testimonials/{testimonialId:guid}/status")]
    public async Task<IActionResult> SetTestimonialStatus(
        Guid testimonialId,
        [FromBody] AdminSetTestimonialStatusDto dto)
    {
        try
        {
            return Ok(await _testimonialService.SetStatusAsync(testimonialId, dto.Status));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("testimonials/{testimonialId:guid}")]
    public async Task<IActionResult> DeleteTestimonial(Guid testimonialId)
    {
        try
        {
            await _testimonialService.DeleteAsync(testimonialId);
            return Ok(new { message = "Testimonial deleted." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
