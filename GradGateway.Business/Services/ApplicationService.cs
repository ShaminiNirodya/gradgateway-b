using GradGateway.Business.DTOs;
using GradGateway.Business.Interfaces;
using GradGateway.Data.Context;
using GradGateway.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GradGateway.Business.Services;

public class ApplicationService : IApplicationService
{
    private readonly GradGatewayDbContext _context;

    public ApplicationService(GradGatewayDbContext context)
    {
        _context = context;
    }

    public async Task<ApplicationResponseDto> ApplyAsync(string firebaseUid, ApplyRequestDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
        if (user == null || user.Role != UserRole.Student)
            throw new InvalidOperationException("Only student users can apply.");

        var student = await _context.StudentProfiles.FirstOrDefaultAsync(s => s.UserId == user.Id);
        if (student == null)
            throw new InvalidOperationException("Student profile not found.");

        var opportunity = await _context.Opportunities
            .Include(o => o.CompanyProfile)
            .FirstOrDefaultAsync(o => o.Id == dto.OpportunityId && o.IsActive);

        if (opportunity == null)
            throw new ArgumentException("Opportunity not found.");

        var existing = await _context.Applications
            .FirstOrDefaultAsync(a => a.OpportunityId == dto.OpportunityId && a.StudentProfileId == student.Id);
        if (existing != null)
            throw new InvalidOperationException("You have already applied to this opportunity.");

        var app = new Application
        {
            Id = Guid.NewGuid(),
            OpportunityId = dto.OpportunityId,
            StudentProfileId = student.Id,
            CoverLetter = dto.CoverLetter,
            Status = ApplicationStatus.Pending,
            AppliedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Applications.Add(app);

        var notif = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = opportunity.CompanyProfile.UserId,
            Type = NotificationType.Application,
            Title = "New Application",
            Body = $"{student.FullName} applied for {opportunity.Title}.",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.Notifications.Add(notif);

        await _context.SaveChangesAsync();

        return new ApplicationResponseDto(
            app.Id,
            opportunity.Id,
            student.Id,
            opportunity.Title,
            opportunity.CompanyProfile.CompanyName,
            student.FullName,
            user.Email,
            app.CoverLetter,
            app.Status.ToString(),
            app.AppliedAt,
            app.UpdatedAt
        );
    }

    public async Task<List<ApplicationResponseDto>> GetStudentApplicationsAsync(string firebaseUid)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
        if (user == null || user.Role != UserRole.Student)
            throw new InvalidOperationException("Only student users can view this list.");

        var student = await _context.StudentProfiles.FirstOrDefaultAsync(s => s.UserId == user.Id);
        if (student == null)
            throw new InvalidOperationException("Student profile not found.");

        var rows = await _context.Applications
            .Include(a => a.Opportunity)
                .ThenInclude(o => o.CompanyProfile)
            .Where(a => a.StudentProfileId == student.Id)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();

        return rows.Select(a => new ApplicationResponseDto(
            a.Id,
            a.OpportunityId,
            student.Id,
            a.Opportunity.Title,
            a.Opportunity.CompanyProfile.CompanyName,
            student.FullName,
            user.Email,
            a.CoverLetter,
            a.Status.ToString(),
            a.AppliedAt,
            a.UpdatedAt
        )).ToList();
    }

    public async Task<List<ApplicationResponseDto>> GetCompanyApplicationsAsync(string firebaseUid)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
        if (user == null || user.Role != UserRole.Company)
            throw new InvalidOperationException("Only company users can view this list.");

        var company = await _context.CompanyProfiles.FirstOrDefaultAsync(c => c.UserId == user.Id);
        if (company == null)
            throw new InvalidOperationException("Company profile not found.");

        var rows = await _context.Applications
            .Include(a => a.Opportunity)
            .Include(a => a.StudentProfile)
                .ThenInclude(s => s.User)
            .Where(a => a.Opportunity.CompanyProfileId == company.Id)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();

        return rows.Select(a => new ApplicationResponseDto(
            a.Id,
            a.OpportunityId,
            a.StudentProfileId,
            a.Opportunity.Title,
            company.CompanyName,
            a.StudentProfile.FullName,
            a.StudentProfile.User.Email,
            a.CoverLetter,
            a.Status.ToString(),
            a.AppliedAt,
            a.UpdatedAt
        )).ToList();
    }

    public async Task<ApplicationResponseDto> UpdateStatusAsync(string firebaseUid, Guid applicationId, string status)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
        if (user == null || user.Role != UserRole.Company)
            throw new InvalidOperationException("Only company users can update application status.");

        var company = await _context.CompanyProfiles.FirstOrDefaultAsync(c => c.UserId == user.Id);
        if (company == null)
            throw new InvalidOperationException("Company profile not found.");

        var app = await _context.Applications
            .Include(a => a.Opportunity)
                .ThenInclude(o => o.CompanyProfile)
            .Include(a => a.StudentProfile)
                .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        if (app == null)
            throw new ArgumentException("Application not found.");

        if (app.Opportunity.CompanyProfileId != company.Id)
            throw new InvalidOperationException("You are not allowed to update this application.");

        if (!Enum.TryParse<ApplicationStatus>(status, true, out var parsed))
            throw new ArgumentException("Invalid application status.");

        app.Status = parsed;
        app.UpdatedAt = DateTime.UtcNow;

        _context.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = app.StudentProfile.UserId,
            Type = NotificationType.Application,
            Title = "Application Status Updated",
            Body = $"Your application for {app.Opportunity.Title} is now {parsed}.",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return new ApplicationResponseDto(
            app.Id,
            app.OpportunityId,
            app.StudentProfileId,
            app.Opportunity.Title,
            app.Opportunity.CompanyProfile.CompanyName,
            app.StudentProfile.FullName,
            app.StudentProfile.User.Email,
            app.CoverLetter,
            app.Status.ToString(),
            app.AppliedAt,
            app.UpdatedAt
        );
    }
}
