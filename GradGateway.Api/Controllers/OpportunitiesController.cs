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

    public OpportunitiesController(IOpportunityService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _service.GetActiveOpportunitiesAsync();
        return Ok(data);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _service.GetOpportunityByIdAsync(id);
        return item == null ? NotFound(new { message = "Opportunity not found" }) : Ok(item);
    }

    [HttpGet("company/me")]
    [Authorize]
    public async Task<IActionResult> GetMyCompanyListings()
    {
        var uid = GetFirebaseUid();
        if (string.IsNullOrWhiteSpace(uid)) return Unauthorized(new { message = "Invalid token" });

        try
        {
            var data = await _service.GetCompanyOpportunitiesAsync(uid);
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

    private string? GetFirebaseUid()
        => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("user_id")?.Value;
}
