using GradGateway.Data.Context;
using GradGateway.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GradGateway.Api.Authorization;

public class AdminRoleHandler : AuthorizationHandler<AdminRoleRequirement>
{
    private readonly GradGatewayDbContext _context;

    public AdminRoleHandler(GradGatewayDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AdminRoleRequirement requirement)
    {
        var firebaseUid = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.User.FindFirst("user_id")?.Value;

        if (string.IsNullOrWhiteSpace(firebaseUid))
        {
            return;
        }

        var user = await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);

        if (user != null && user.Role == UserRole.Admin && user.IsActive)
        {
            context.Succeed(requirement);
        }
    }
}
