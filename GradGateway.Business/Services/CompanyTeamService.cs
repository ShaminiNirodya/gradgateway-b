using System.Security.Cryptography;
using GradGateway.Business.DTOs;
using GradGateway.Business.Interfaces;
using GradGateway.Data.Context;
using GradGateway.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GradGateway.Business.Services;

public class CompanyTeamService : ICompanyTeamService
{
    private readonly GradGatewayDbContext _context;

    public CompanyTeamService(GradGatewayDbContext context)
    {
        _context = context;
    }

    public async Task<TeamMemberResponseDto> InviteMemberAsync(string firebaseUid, InviteTeamMemberRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Role))
            throw new InvalidOperationException("Name, email and role are required.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid)
                   ?? throw new InvalidOperationException("Company user not found.");

        if (user.Role != UserRole.Company)
            throw new InvalidOperationException("Only company users can invite team members.");

        var companyProfile = await _context.CompanyProfiles.FirstOrDefaultAsync(c => c.UserId == user.Id)
            ?? throw new InvalidOperationException("Company profile not found.");

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var existing = await _context.CompanyTeamMembers
            .FirstOrDefaultAsync(m => m.CompanyProfileId == companyProfile.Id && m.Email == normalizedEmail);

        if (existing != null)
        {
            if (existing.Status == "Removed")
            {
                existing.Name = dto.Name.Trim();
                existing.Role = dto.Role.Trim();
                existing.Status = "Pending";
                existing.InvitedAt = DateTime.UtcNow;
                existing.AcceptedAt = null;
                existing.InvitationToken = CreateToken();
                existing.InvitationExpiresAt = DateTime.UtcNow.AddDays(7);
                existing.InvitedByUserId = user.Id;
                await _context.SaveChangesAsync();
                return Map(existing);
            }

            throw new InvalidOperationException("This email is already invited or active in your team.");
        }

        var member = new CompanyTeamMember
        {
            Id = Guid.NewGuid(),
            CompanyProfileId = companyProfile.Id,
            Name = dto.Name.Trim(),
            Email = normalizedEmail,
            Role = dto.Role.Trim(),
            Status = "Pending",
            InvitationToken = CreateToken(),
            InvitationExpiresAt = DateTime.UtcNow.AddDays(7),
            InvitedAt = DateTime.UtcNow,
            InvitedByUserId = user.Id,
        };

        _context.CompanyTeamMembers.Add(member);
        await _context.SaveChangesAsync();

        return Map(member);
    }

    public async Task<List<TeamMemberResponseDto>> GetMembersAsync(string firebaseUid)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid)
                   ?? throw new InvalidOperationException("Company user not found.");

        var companyProfile = await _context.CompanyProfiles.FirstOrDefaultAsync(c => c.UserId == user.Id)
            ?? throw new InvalidOperationException("Company profile not found.");

        var members = await _context.CompanyTeamMembers
            .Where(m => m.CompanyProfileId == companyProfile.Id && m.Status != "Removed")
            .OrderByDescending(m => m.InvitedAt)
            .ToListAsync();

        return members.Select(Map).ToList();
    }

    public async Task RemoveMemberAsync(string firebaseUid, Guid memberId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid)
                   ?? throw new InvalidOperationException("Company user not found.");

        var companyProfile = await _context.CompanyProfiles.FirstOrDefaultAsync(c => c.UserId == user.Id)
            ?? throw new InvalidOperationException("Company profile not found.");

        var member = await _context.CompanyTeamMembers
            .FirstOrDefaultAsync(m => m.Id == memberId && m.CompanyProfileId == companyProfile.Id)
            ?? throw new InvalidOperationException("Team member not found.");

        member.Status = "Removed";
        await _context.SaveChangesAsync();
    }

    public async Task<TeamMemberResponseDto> AcceptInviteAsync(AcceptTeamInviteRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Token))
            throw new InvalidOperationException("Invitation token is required.");

        var member = await _context.CompanyTeamMembers
            .FirstOrDefaultAsync(m => m.InvitationToken == dto.Token.Trim())
            ?? throw new InvalidOperationException("Invitation is invalid.");

        if (member.Status != "Pending")
            throw new InvalidOperationException("Invitation is no longer pending.");

        if (member.InvitationExpiresAt < DateTime.UtcNow)
            throw new InvalidOperationException("Invitation has expired.");

        member.Status = "Active";
        member.AcceptedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Map(member);
    }

    private static TeamMemberResponseDto Map(CompanyTeamMember member)
        => new(
            member.Id,
            member.Name,
            member.Email,
            member.Role,
            member.Status,
            member.InvitedAt,
            member.AcceptedAt
        );

    private static string CreateToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(48))
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
}
