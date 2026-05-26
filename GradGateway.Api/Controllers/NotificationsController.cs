using GradGateway.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GradGateway.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationsController(INotificationService service)
    {
        _service = service;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMine()
    {
        var uid = GetFirebaseUid();
        if (string.IsNullOrWhiteSpace(uid)) return Unauthorized(new { message = "Invalid token" });

        var data = await _service.GetMyNotificationsAsync(uid);
        return Ok(data);
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        var uid = GetFirebaseUid();
        if (string.IsNullOrWhiteSpace(uid)) return Unauthorized(new { message = "Invalid token" });

        try
        {
            await _service.MarkAsReadAsync(uid, id);
            return Ok(new { message = "Notification marked as read" });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private string? GetFirebaseUid()
        => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("user_id")?.Value;
}
