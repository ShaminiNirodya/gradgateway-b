using System.Security.Claims;
using GradGateway.Business.DTOs;
using GradGateway.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GradGateway.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompanyTeamController : ControllerBase
{
    private readonly ICompanyTeamService _companyTeamService;

    public CompanyTeamController(ICompanyTeamService companyTeamService)
    {
        _companyTeamService = companyTeamService;
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMyTeam()
    {
        var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("user_id")?.Value;

        if (string.IsNullOrWhiteSpace(firebaseUid))
            return Unauthorized(new { message = "Invalid token: Firebase UID not found" });

        try
        {
            var members = await _companyTeamService.GetMembersAsync(firebaseUid);
            return Ok(members);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("invite")]
    [Authorize]
    public async Task<IActionResult> InviteMember([FromBody] InviteTeamMemberRequestDto dto)
    {
        var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("user_id")?.Value;

        if (string.IsNullOrWhiteSpace(firebaseUid))
            return Unauthorized(new { message = "Invalid token: Firebase UID not found" });

        try
        {
            var member = await _companyTeamService.InviteMemberAsync(firebaseUid, dto);
            return Ok(member);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{memberId:guid}/remove")]
    [Authorize]
    public async Task<IActionResult> RemoveMember([FromRoute] Guid memberId)
    {
        var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("user_id")?.Value;

        if (string.IsNullOrWhiteSpace(firebaseUid))
            return Unauthorized(new { message = "Invalid token: Firebase UID not found" });

        try
        {
            await _companyTeamService.RemoveMemberAsync(firebaseUid, memberId);
            return Ok(new { message = "Team member removed successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("accept")]
    [AllowAnonymous]
    public async Task<IActionResult> AcceptInvite([FromBody] AcceptTeamInviteRequestDto dto)
    {
        try
        {
            var member = await _companyTeamService.AcceptInviteAsync(dto);
            return Ok(member);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
