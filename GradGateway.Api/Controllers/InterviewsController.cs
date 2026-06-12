using GradGateway.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GradGateway.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InterviewsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public InterviewsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    /// <summary>Upcoming and past interviews for the current student.</summary>
    [HttpGet("student/me")]
    public async Task<IActionResult> GetMyInterviews()
    {
        var uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("user_id")?.Value;
        if (string.IsNullOrWhiteSpace(uid))
        {
            return Unauthorized(new { message = "Invalid token: Firebase UID not found" });
        }

        try
        {
            return Ok(await _studentService.GetMyInterviewsAsync(uid));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
