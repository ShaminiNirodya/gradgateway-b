using GradGateway.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GradGateway.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _service;

    public ProjectsController(IProjectService service)
    {
        _service = service;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMine()
    {
        var uid = GetFirebaseUid();
        if (string.IsNullOrWhiteSpace(uid)) return Unauthorized(new { message = "Invalid token" });

        try
        {
            var result = await _service.GetMyProjectsAsync(uid);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("me/{id:guid}")]
    public async Task<IActionResult> GetMineById(Guid id)
    {
        var uid = GetFirebaseUid();
        if (string.IsNullOrWhiteSpace(uid)) return Unauthorized(new { message = "Invalid token" });

        try
        {
            var result = await _service.GetMyProjectByIdAsync(uid, id);
            return result == null ? NotFound(new { message = "Project not found" }) : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private string? GetFirebaseUid()
        => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("user_id")?.Value;
}
