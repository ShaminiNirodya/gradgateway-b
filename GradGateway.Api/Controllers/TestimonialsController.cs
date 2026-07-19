using GradGateway.Business.DTOs;
using GradGateway.Business.Interfaces;
using GradGateway.Data.Context;
using GradGateway.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GradGateway.Api.Controllers;

[ApiController]
[Route("api/testimonials")]
public class TestimonialsController : ControllerBase
{
    private readonly ITestimonialService _service;
    private readonly GradGatewayDbContext _context;

    public TestimonialsController(ITestimonialService service, GradGatewayDbContext context)
    {
        _service = service;
        _context = context;
    }

    [HttpPost("submit")]
    [Authorize]
    public async Task<IActionResult> Submit([FromBody] SubmitTestimonialDto dto)
    {
        try
        {
            var firebaseUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(firebaseUid))
            {
                return Unauthorized(new { message = "Authentication required." });
            }

            var user = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);

            if (user is null || !user.IsActive)
            {
                return Unauthorized(new { message = "Account not found or inactive." });
            }

            if (user.Role is not (UserRole.Student or UserRole.Company))
            {
                return Forbid();
            }

            var submitterRole = user.Role == UserRole.Student ? "Student" : "Company";
            var submitDto = dto with
            {
                Email = string.IsNullOrWhiteSpace(dto.Email) ? user.Email : dto.Email,
                SubmitterRole = submitterRole,
            };

            var row = await _service.SubmitAsync(submitDto, user.Id);
            return Ok(new
            {
                message = "Thanks for sharing your experience. Our team will review it before it appears on the homepage.",
                testimonial = row,
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
