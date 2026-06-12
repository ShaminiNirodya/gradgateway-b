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
    private readonly IProjectService _projectService;
    private readonly ILogger<StudentsController> _logger;

    public StudentsController(
        IStudentService studentService,
        IProjectService projectService,
        ILogger<StudentsController> logger)
    {
        _studentService = studentService;
        _projectService = projectService;
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

    [HttpGet("{studentProfileId:guid}/projects")]
    [Authorize]
    public async Task<IActionResult> GetStudentProjects(Guid studentProfileId)
    {
        try
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("user_id")?.Value;

            if (string.IsNullOrWhiteSpace(firebaseUid))
            {
                return Unauthorized(new { message = "Invalid token: Firebase UID not found" });
            }

            var result = await _projectService.GetProjectsByStudentProfileIdAsync(firebaseUid, studentProfileId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading projects for student {StudentProfileId}", studentProfileId);
            return StatusCode(500, new { message = "An error occurred while retrieving student projects" });
        }
    }

    [HttpGet("{studentProfileId:guid}/directory-entry")]
    [Authorize]
    public async Task<IActionResult> GetStudentDirectoryEntry(Guid studentProfileId)
    {
        try
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("user_id")?.Value;

            if (string.IsNullOrWhiteSpace(firebaseUid))
            {
                return Unauthorized(new { message = "Invalid token: Firebase UID not found" });
            }

            var entry = await _studentService.GetStudentDirectoryItemByProfileIdAsync(studentProfileId);
            if (entry == null)
            {
                return NotFound(new { message = "Student not found" });
            }

            return Ok(entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student directory entry {StudentProfileId}", studentProfileId);
            return StatusCode(500, new { message = "An error occurred while retrieving student profile" });
        }
    }

    [HttpGet("me/skills")]
    [Authorize]
    public async Task<IActionResult> GetMySkills()
    {
        var firebaseUid = GetFirebaseUid();
        if (string.IsNullOrWhiteSpace(firebaseUid))
        {
            return Unauthorized(new { message = "Invalid token: Firebase UID not found" });
        }

        try
        {
            return Ok(await _studentService.GetMySkillsAsync(firebaseUid));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("me/skills")]
    [Authorize]
    public async Task<IActionResult> AddMySkill([FromBody] AddStudentSkillDto dto)
    {
        var firebaseUid = GetFirebaseUid();
        if (string.IsNullOrWhiteSpace(firebaseUid))
        {
            return Unauthorized(new { message = "Invalid token: Firebase UID not found" });
        }

        try
        {
            return Ok(await _studentService.AddSkillAsync(firebaseUid, dto));
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

    [HttpDelete("me/skills/{studentSkillId:guid}")]
    [Authorize]
    public async Task<IActionResult> RemoveMySkill(Guid studentSkillId)
    {
        var firebaseUid = GetFirebaseUid();
        if (string.IsNullOrWhiteSpace(firebaseUid))
        {
            return Unauthorized(new { message = "Invalid token: Firebase UID not found" });
        }

        try
        {
            await _studentService.RemoveSkillAsync(firebaseUid, studentSkillId);
            return Ok(new { message = "Skill removed." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private string? GetFirebaseUid()
        => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("user_id")?.Value;

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
