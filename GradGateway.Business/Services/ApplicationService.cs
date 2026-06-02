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

        var todayUtc = DateTime.UtcNow.Date;
        var opportunity = await _context.Opportunities
            .Include(o => o.CompanyProfile)
            .FirstOrDefaultAsync(o => o.Id == dto.OpportunityId);

        if (opportunity == null || !opportunity.IsActive)
            throw new ArgumentException("Opportunity not found.");

        if (opportunity.DeadlineAt.Date < todayUtc)
            throw new ArgumentException("This job post has expired and is no longer accepting applications.");

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
                .ThenInclude(o => o!.CompanyProfile)
            .Include(a => a.CompanyProfile)
            .Where(a => a.StudentProfileId == student.Id)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();

        return rows.Select(a =>
        {
            string jobTitle;
            string companyName;
            
            if (a.Opportunity != null)
            {
                jobTitle = a.Opportunity.Title;
                companyName = a.Opportunity.CompanyProfile.CompanyName;
            }
            else
            {
                jobTitle = a.JobTitle ?? "Direct Job Offer";
                companyName = a.CompanyProfile?.CompanyName ?? "Unknown Company";
            }

            return new ApplicationResponseDto(
                a.Id,
                a.OpportunityId,
                student.Id,
                jobTitle,
                companyName,
                student.FullName,
                user.Email,
                a.CoverLetter,
                a.Status.ToString(),
                a.AppliedAt,
                a.UpdatedAt
            );
        }).ToList();
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
            .Where(a => (a.Opportunity != null && a.Opportunity.CompanyProfileId == company.Id) ||
                       (a.CompanyProfileId == company.Id))
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();

        return rows.Select(a =>
        {
            string jobTitle = a.Opportunity != null ? a.Opportunity.Title : (a.JobTitle ?? "Direct Job Offer");
            
            return new ApplicationResponseDto(
                a.Id,
                a.OpportunityId,
                a.StudentProfileId,
                jobTitle,
                company.CompanyName,
                a.StudentProfile.FullName,
                a.StudentProfile.User.Email,
                a.CoverLetter,
                a.Status.ToString(),
                a.AppliedAt,
                a.UpdatedAt
            );
        }).ToList();
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
                .ThenInclude(o => o!.CompanyProfile)
            .Include(a => a.CompanyProfile)
            .Include(a => a.StudentProfile)
                .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        if (app == null)
            throw new ArgumentException("Application not found.");

        var companyId = app.Opportunity?.CompanyProfileId ?? app.CompanyProfileId;
        if (companyId != company.Id)
            throw new InvalidOperationException("You are not allowed to update this application.");

        if (!Enum.TryParse<ApplicationStatus>(status, true, out var parsed))
            throw new ArgumentException("Invalid application status.");

        app.Status = parsed;
        app.UpdatedAt = DateTime.UtcNow;

        string jobTitle = app.Opportunity?.Title ?? app.JobTitle ?? "Direct Job Offer";
        string companyName = app.Opportunity?.CompanyProfile?.CompanyName ?? app.CompanyProfile?.CompanyName ?? company.CompanyName;

        _context.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = app.StudentProfile.UserId,
            Type = NotificationType.Application,
            Title = "Application Status Updated",
            Body = $"Your application for {jobTitle} is now {parsed}.",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return new ApplicationResponseDto(
            app.Id,
            app.OpportunityId,
            app.StudentProfileId,
            jobTitle,
            companyName,
            app.StudentProfile.FullName,
            app.StudentProfile.User.Email,
            app.CoverLetter,
            app.Status.ToString(),
            app.AppliedAt,
            app.UpdatedAt
        );
    }

    public async Task<ApplicationResponseDto> CreateJobOfferApplicationAsync(
        string firebaseUid,
        Guid studentProfileId,
        string jobTitle,
        string jobType,
        string? compensation,
        string proposalMessage)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
        if (user == null || user.Role != UserRole.Company)
            throw new InvalidOperationException("Only company users can send job offers.");

        var company = await _context.CompanyProfiles.FirstOrDefaultAsync(c => c.UserId == user.Id);
        if (company == null)
            throw new InvalidOperationException("Company profile not found.");

        var student = await _context.StudentProfiles
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == studentProfileId);
        if (student == null)
            throw new ArgumentException("Student not found.");

        var app = new Application
        {
            Id = Guid.NewGuid(),
            OpportunityId = null,
            CompanyProfileId = company.Id,
            StudentProfileId = student.Id,
            CoverLetter = proposalMessage,
            JobTitle = jobTitle,
            JobType = jobType,
            Compensation = compensation,
            Status = ApplicationStatus.OfferSent,
            AppliedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Applications.Add(app);

        var notif = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = student.UserId,
            Type = NotificationType.Application,
            Title = "Job Offer Received",
            Body = $"{company.CompanyName} sent you a job offer for {jobTitle}.",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.Notifications.Add(notif);

        await _context.SaveChangesAsync();

        return new ApplicationResponseDto(
            app.Id,
            null,
            student.Id,
            jobTitle,
            company.CompanyName,
            student.FullName,
            student.User.Email,
            app.CoverLetter,
            app.Status.ToString(),
            app.AppliedAt,
            app.UpdatedAt
        );
    }
}
