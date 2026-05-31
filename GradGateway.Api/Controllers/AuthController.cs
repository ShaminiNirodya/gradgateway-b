using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GradGateway.Business.DTOs;
using GradGateway.Business.Interfaces;
using System.Security.Claims;

namespace GradGateway.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Syncs the Firebase authenticated user with the backend database
    /// This endpoint is called after Firebase login to create/retrieve user profile
    /// </summary>
    [HttpPost("sync")]
    [Authorize] // Requires valid Firebase Token
    public async Task<IActionResult> SyncUser([FromBody] UserRegistrationDto dto)
    {
        try
        {
            // Get the Firebase UID from the JWT token claims
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                           ?? User.FindFirst("user_id")?.Value;

            if (string.IsNullOrEmpty(firebaseUid))
            {
                return Unauthorized(new { message = "Invalid token: Firebase UID not found" });
            }

            // Verify that the UID in the token matches the DTO
            if (firebaseUid != dto.FirebaseUid)
            {
                return BadRequest(new { message = "Firebase UID mismatch" });
            }

            var result = await _userService.GetOrCreateUserAsync(dto);
            
            if (result == null)
            {
                return BadRequest(new { message = "Failed to sync user" });
            }

            _logger.LogInformation("User synced successfully: {Email} with role {Role}", result.Email, result.Role);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing user");
            return StatusCode(500, new { message = "An error occurred while syncing user" });
        }
    }

    /// <summary>
    /// Gets the current authenticated user's profile
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                           ?? User.FindFirst("user_id")?.Value;

            if (string.IsNullOrEmpty(firebaseUid))
            {
                return Unauthorized(new { message = "Invalid token: Firebase UID not found" });
            }

            var user = await _userService.GetUserByFirebaseUidAsync(firebaseUid);
            
            if (user == null)
            {
                return NotFound(new { message = "User not found. Please sync your account first." });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(500, new { message = "An error occurred while retrieving user data" });
        }
    }

    /// <summary>
    /// Health check endpoint (no authentication required)
    /// </summary>
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Database check endpoint - verifies database connectivity and tables
    /// </summary>
    [HttpGet("db-check")]
    public async Task<IActionResult> DatabaseCheck()
    {
        try
        {
            var userCount = await _userService.GetUserCountAsync();
            return Ok(new 
            { 
                status = "database_connected", 
                userCount = userCount,
                message = "Database tables are accessible",
                timestamp = DateTime.UtcNow 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database check failed");
            return StatusCode(500, new 
            { 
                status = "database_error", 
                message = ex.Message,
                timestamp = DateTime.UtcNow 
            });
        }
    }

    /// <summary>
    /// Request a password reset code to be sent to the user's email
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                return BadRequest(new { message = "Email is required" });
            }

            var result = await _userService.RequestPasswordResetAsync(dto.Email);
            
            _logger.LogInformation("Password reset requested for email: {Email}", dto.Email);

            if (!result.Success)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing forgot password request");
            return StatusCode(500, new { message = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Verify a password reset code
    /// </summary>
    [HttpPost("verify-reset-code")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyResetCode([FromBody] VerifyResetCodeRequestDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Code))
            {
                return BadRequest(new { message = "Email and code are required" });
            }

            var result = await _userService.VerifyResetCodeAsync(dto.Email, dto.Code);
            
            if (result.Valid)
            {
                _logger.LogInformation("Password reset code verified for email: {Email}", dto.Email);
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying reset code");
            return StatusCode(500, new { message = "An error occurred while verifying the reset code" });
        }
    }

    /// <summary>
    /// Reset password using verification code and update Firebase Auth password
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Code))
            {
                return BadRequest(new { message = "Email and code are required" });
            }

            if (string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                return BadRequest(new { message = "New password is required" });
            }

            var result = await _userService.ResetPasswordAsync(dto.Email, dto.Code, dto.NewPassword);
            
            if (result.Success)
            {
                _logger.LogInformation("Password reset successful for email: {Email}", dto.Email);
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return StatusCode(500, new { message = "An error occurred while resetting the password" });
        }
    }
}
