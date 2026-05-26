using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GradGateway.Business.DTOs;
using GradGateway.Business.Interfaces;

namespace GradGateway.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly ILogger<StudentsController> _logger;

    public StudentsController(IStudentService studentService, ILogger<StudentsController> logger)
    {
        _studentService = studentService;
        _logger = logger;
    }

    [HttpPost("register")]
    [Authorize]
    public async Task<IActionResult> RegisterStudent([FromBody] StudentRegistrationDto dto)
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

            var result = await _studentService.RegisterOrUpdateStudentAsync(dto);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering student");
            return StatusCode(500, new { message = "An error occurred while registering student" });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentStudent()
    {
        try
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("user_id")?.Value;

            if (string.IsNullOrWhiteSpace(firebaseUid))
            {
                return Unauthorized(new { message = "Invalid token: Firebase UID not found" });
            }

            var student = await _studentService.GetStudentByFirebaseUidAsync(firebaseUid);
            if (student == null)
            {
                return NotFound(new { message = "Student profile not found" });
            }

            return Ok(student);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student profile");
            return StatusCode(500, new { message = "An error occurred while retrieving student profile" });
        }
    }

    [HttpGet("directory")]
    [Authorize]
    public async Task<IActionResult> GetStudentDirectory([FromQuery] string? q = null)
    {
        try
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("user_id")?.Value;

            if (string.IsNullOrWhiteSpace(firebaseUid))
            {
                return Unauthorized(new { message = "Invalid token: Firebase UID not found" });
            }

            var rows = await _studentService.GetStudentDirectoryAsync(q);
            return Ok(rows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student directory");
            return StatusCode(500, new { message = "An error occurred while retrieving student directory" });
        }
    }
}
