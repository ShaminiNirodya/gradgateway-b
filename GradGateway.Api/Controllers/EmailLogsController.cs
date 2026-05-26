using GradGateway.Business.DTOs;
using GradGateway.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GradGateway.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmailLogsController : ControllerBase
{
    private readonly IEmailLogService _service;

    public EmailLogsController(IEmailLogService service)
    {
        _service = service;
    }

    [HttpPost("track")]
    public async Task<IActionResult> Track([FromBody] EmailLogTrackRequestDto dto)
    {
        var uid = GetFirebaseUid();
        if (string.IsNullOrWhiteSpace(uid)) return Unauthorized(new { message = "Invalid token" });

        if (string.IsNullOrWhiteSpace(dto.ToEmail) || string.IsNullOrWhiteSpace(dto.TemplateType) || string.IsNullOrWhiteSpace(dto.Purpose))
        {
            return BadRequest(new { message = "ToEmail, TemplateType and Purpose are required." });
        }

        try
        {
            var row = await _service.TrackAsync(uid, dto);
            return Ok(row);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMine([FromQuery] int take = 100)
    {
        var uid = GetFirebaseUid();
        if (string.IsNullOrWhiteSpace(uid)) return Unauthorized(new { message = "Invalid token" });

        try
        {
            var data = await _service.GetMyLogsAsync(uid, take);
            return Ok(data);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private string? GetFirebaseUid()
        => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("user_id")?.Value;
}
