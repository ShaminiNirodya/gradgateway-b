using GradGateway.Business.DTOs;
using GradGateway.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GradGateway.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OpportunitiesController : ControllerBase
{
    private readonly IOpportunityService _service;
    private readonly IInterviewPlanService _interviewPlanService;
    private readonly ICompanyService _companyService;

    public OpportunitiesController(
        IOpportunityService service,
        IInterviewPlanService interviewPlanService,
        ICompanyService companyService)
    {
        _service = service;
        _interviewPlanService = interviewPlanService;
        _companyService = companyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = Pagination.DefaultPageSize)
    {
        var feed = await _service.GetStudentOpeningsFeedAsync(page, pageSize);
        return Ok(feed);
    }

    [HttpGet("expired-count")]
    public async Task<IActionResult> GetExpiredCount()
    {
        var count = await _service.GetExpiredOpportunitiesCountAsync();
        return Ok(new { count });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _service.GetOpportunityByIdAsync(id);
        return item == null ? NotFound(new { message = "Opportunity not found" }) : Ok(item);
    }

    /// <summary>
    /// Public company profile for the employer behind a job listing (student view).
    /// </summary>
    [HttpGet("{id:guid}/company-profile")]
    [Authorize]
    public async Task<IActionResult> GetCompanyProfileForOpportunity(Guid id)
    {
        var uid = GetFirebaseUid();
        if (string.IsNullOrWhiteSpace(uid))
        {
            return Unauthorized(new { message = "Invalid token: Firebase UID not found" });
        }

        var opportunity = await _service.GetOpportunityByIdAsync(id);
        if (opportunity == null)
        {
            return NotFound(new { message = "Opportunity not found" });
        }

        var profile = await _companyService.GetPublicCompanyProfileAsync(opportunity.CompanyProfileId);
        if (profile == null)
        {
            return NotFound(new { message = "Company profile not found" });
        }

        return Ok(profile);
    }

    [HttpGet("company/me")]
    [Authorize]
    public async Task<IActionResult> GetMyCompanyListings(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize)
    {
        var uid = GetFirebaseUid();
        if (string.IsNullOrWhiteSpace(uid)) return Unauthorized(new { message = "Invalid token" });

        try
        {
            var data = await _service.GetCompanyOpportunitiesAsync(uid, page, pageSize);
            return Ok(data);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateOpportunityRequestDto dto)
    {
        var uid = GetFirebaseUid();
        if (string.IsNullOrWhiteSpace(uid)) return Unauthorized(new { message = "Invalid token" });

        try
        {
            var created = await _service.CreateOpportunityAsync(uid, dto);
            return Ok(created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOpportunityRequestDto dto)
    {
        var uid = GetFirebaseUid();
        if (string.IsNullOrWhiteSpace(uid)) return Unauthorized(new { message = "Invalid token" });

        try
        {
            var updated = await _service.UpdateOpportunityAsync(uid, id, dto);
            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/close")]
    [Authorize]
    public async Task<IActionResult> Close(Guid id)
    {
        var uid = GetFirebaseUid();
        if (string.IsNullOrWhiteSpace(uid)) return Unauthorized(new { message = "Invalid token" });

        try
        {
            var closed = await _service.CloseOpportunityAsync(uid, id);
            return Ok(closed);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var uid = GetFirebaseUid();
        if (string.IsNullOrWhiteSpace(uid)) return Unauthorized(new { message = "Invalid token" });

        try
        {
            await _service.DeleteOpportunityAsync(uid, id);
            return Ok(new { message = "Job post deleted." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}/interview-plan")]
    [Authorize]
    public async Task<IActionResult> GetInterviewPlan(Guid id)
    {
        var uid = GetFirebaseUid();
        if (string.IsNullOrWhiteSpace(uid)) return Unauthorized(new { message = "Invalid token" });

        try
        {
            var plan = await _interviewPlanService.GetPlanAsync(uid, id);
            return plan == null ? Ok(null) : Ok(plan);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/schedule-interviews")]
    [Authorize]
    public async Task<IActionResult> ScheduleInterviews(Guid id, [FromBody] ScheduleInterviewsRequestDto dto)
    {
        var uid = GetFirebaseUid();
        if (string.IsNullOrWhiteSpace(uid)) return Unauthorized(new { message = "Invalid token" });

        try
        {
            var result = await _service.ScheduleInterviewsAsync(uid, id, dto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private string? GetFirebaseUid()
        => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("user_id")?.Value;
}
