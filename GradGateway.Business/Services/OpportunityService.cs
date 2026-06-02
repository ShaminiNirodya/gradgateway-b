using GradGateway.Business.DTOs;
using GradGateway.Business.Interfaces;
using GradGateway.Data.Context;
using GradGateway.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GradGateway.Business.Services;

public class OpportunityService : IOpportunityService
{
    private readonly GradGatewayDbContext _context;

    public OpportunityService(GradGatewayDbContext context)
    {
        _context = context;
    }

    public async Task<OpportunityResponseDto> CreateOpportunityAsync(string firebaseUid, CreateOpportunityRequestDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
        if (user == null || user.Role != UserRole.Company)
            throw new InvalidOperationException("Only company users can create opportunities.");

        var company = await _context.CompanyProfiles.FirstOrDefaultAsync(c => c.UserId == user.Id);
        if (company == null)
            throw new InvalidOperationException("Company profile not found.");

        if (!Enum.TryParse<OpportunityType>(dto.OpportunityType, true, out var type))
            throw new ArgumentException("Invalid opportunity type.");

        if (!Enum.TryParse<WorkMode>(dto.WorkMode, true, out var mode))
            throw new ArgumentException("Invalid work mode.");

        var deadlineUtc = DateTime.SpecifyKind(dto.DeadlineAt.Date, DateTimeKind.Utc);
        if (deadlineUtc < DateTime.UtcNow.Date)
            throw new ArgumentException("Deadline must be today or a future date.");

        var opportunity = new Opportunity
        {
            Id = Guid.NewGuid(),
            CompanyProfileId = company.Id,
            Title = dto.Title,
            Description = dto.Description,
            OpportunityType = type,
            WorkMode = mode,
            Location = dto.Location,
            RequiredSkills = dto.RequiredSkills,
            MonthlyStipendLkr = dto.MonthlyStipendLkr,
            DeadlineAt = deadlineUtc,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        return ToResponse(opportunity, company.CompanyName, company.LogoDataUrl);
    }

    public async Task<List<OpportunityResponseDto>> GetActiveOpportunitiesAsync()
    {
        await EnsureDemoOpportunitiesAsync();
        await AutoExpireOpportunitiesAsync();

        var todayUtc = DateTime.UtcNow.Date;

        var rows = await _context.Opportunities
            .Include(o => o.CompanyProfile)
            .Where(o => o.IsActive && o.DeadlineAt.Date >= todayUtc)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return rows.Select(o => ToResponse(o, o.CompanyProfile.CompanyName, o.CompanyProfile.LogoDataUrl)).ToList();
    }

    public async Task<List<OpportunityResponseDto>> GetCompanyOpportunitiesAsync(string firebaseUid)
    {
        await AutoExpireOpportunitiesAsync();
        await SendDeadlinePassedNotificationsAsync();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
        if (user == null || user.Role != UserRole.Company)
            throw new InvalidOperationException("Only company users can access company opportunities.");

        var company = await _context.CompanyProfiles.FirstOrDefaultAsync(c => c.UserId == user.Id);
        if (company == null)
            throw new InvalidOperationException("Company profile not found.");

        var rows = await _context.Opportunities
            .Where(o => o.CompanyProfileId == company.Id)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return rows.Select(o => ToResponse(o, company.CompanyName, company.LogoDataUrl)).ToList();
    }

    public async Task<OpportunityResponseDto?> GetOpportunityByIdAsync(Guid id)
    {
        var row = await _context.Opportunities
            .Include(o => o.CompanyProfile)
            .FirstOrDefaultAsync(o => o.Id == id);

        return row == null ? null : ToResponse(row, row.CompanyProfile.CompanyName, row.CompanyProfile.LogoDataUrl);
    }

    private static OpportunityResponseDto ToResponse(Opportunity o, string companyName, string? companyLogoUrl = null)
    {
        return new OpportunityResponseDto(
            o.Id,
            o.CompanyProfileId,
            companyName,
            companyLogoUrl,
            o.Title,
            o.Description,
            o.OpportunityType.ToString(),
            o.WorkMode.ToString(),
            o.Location,
            o.RequiredSkills,
            o.MonthlyStipendLkr,
            o.DeadlineAt,
            o.IsActive,
            o.CreatedAt
        );
    }

    private async Task EnsureDemoOpportunitiesAsync()
    {
        if (await _context.Opportunities.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;

        var companyProfiles = await _context.CompanyProfiles
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        if (!companyProfiles.Any())
        {
            return;
        }

        var seeds = new[]
        {
            new { Title = "Software Engineering Intern", Desc = "Build internal tools with .NET and React.", Skills = "C#, ASP.NET Core, React, SQL Server", Type = OpportunityType.Internship, Mode = WorkMode.Hybrid, Location = "Colombo", Stipend = 80000m, Days = 45 },
            new { Title = "Data Analytics Intern", Desc = "Work with telecom data and BI pipelines.", Skills = "Python, SQL, Power BI, Statistics", Type = OpportunityType.Internship, Mode = WorkMode.Onsite, Location = "Battaramulla", Stipend = 90000m, Days = 55 },
            new { Title = "Associate Software Engineer", Desc = "Graduate role for cloud-native service development.", Skills = "Java, Microservices, Docker, Kubernetes", Type = OpportunityType.GraduateRole, Mode = WorkMode.Hybrid, Location = "Colombo", Stipend = 185000m, Days = 65 },
            new { Title = "QA Automation Intern", Desc = "Automate regression suites with Playwright and CI.", Skills = "Playwright, Selenium, C#, CI/CD", Type = OpportunityType.Internship, Mode = WorkMode.Remote, Location = "Sri Lanka", Stipend = 70000m, Days = 50 }
        };

        for (var i = 0; i < seeds.Length; i++)
        {
            var company = companyProfiles[i % companyProfiles.Count];
            _context.Opportunities.Add(new Opportunity
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = company.Id,
                Title = seeds[i].Title,
                Description = seeds[i].Desc,
                OpportunityType = seeds[i].Type,
                WorkMode = seeds[i].Mode,
                Location = seeds[i].Location,
                RequiredSkills = seeds[i].Skills,
                MonthlyStipendLkr = seeds[i].Stipend,
                DeadlineAt = now.AddDays(seeds[i].Days),
                IsActive = true,
                CreatedAt = now.AddDays(-(i + 1) * 2),
                UpdatedAt = now.AddDays(-(i + 1))
            });
        }

        await _context.SaveChangesAsync();
    }

    private async Task AutoExpireOpportunitiesAsync()
    {
        var now = DateTime.UtcNow;

        var todayUtc = now.Date;
        var expiredActiveRows = await _context.Opportunities
            .Where(o => o.IsActive && o.DeadlineAt.Date < todayUtc)
            .ToListAsync();

        if (!expiredActiveRows.Any())
        {
            return;
        }

        foreach (var item in expiredActiveRows)
        {
            item.IsActive = false;
            item.UpdatedAt = now;
        }

        await _context.SaveChangesAsync();
        await SendDeadlinePassedNotificationsAsync();
    }

    public async Task<ScheduleInterviewsResultDto> ScheduleInterviewsAsync(
        string firebaseUid,
        Guid opportunityId,
        ScheduleInterviewsRequestDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid)
                   ?? throw new InvalidOperationException("User not found.");

        if (user.Role != UserRole.Company)
            throw new InvalidOperationException("Only company users can schedule interviews.");

        var company = await _context.CompanyProfiles.FirstOrDefaultAsync(c => c.UserId == user.Id)
                      ?? throw new InvalidOperationException("Company profile not found.");

        var opportunity = await _context.Opportunities
            .Include(o => o.CompanyProfile)
            .FirstOrDefaultAsync(o => o.Id == opportunityId && o.CompanyProfileId == company.Id)
            ?? throw new ArgumentException("Opportunity not found.");

        if (!Enum.TryParse<InterviewMode>(dto.Mode, true, out var interviewMode))
            throw new ArgumentException("Invalid interview mode. Use Online, Onsite, or Phone.");

        var scheduledAt = DateTime.SpecifyKind(dto.ScheduledAt.Date, DateTimeKind.Utc);
        if (scheduledAt < DateTime.UtcNow.Date)
            throw new ArgumentException("Interview date must be today or in the future.");

        var durationMinutes = dto.DurationMinutes is > 0 and <= 480 ? dto.DurationMinutes : 60;

        var shortlisted = await _context.Applications
            .Include(a => a.StudentProfile)
            .Where(a => a.OpportunityId == opportunityId && a.Status == ApplicationStatus.Shortlisted)
            .ToListAsync();

        var messagesSent = 0;
        var interviewsScheduled = 0;
        var companyName = opportunity.CompanyProfile.CompanyName;
        var messageContent = BuildInterviewChatMessage(
            opportunity.Title,
            companyName,
            scheduledAt,
            durationMinutes,
            interviewMode,
            dto.MeetingLink,
            dto.Location,
            dto.Notes);

        foreach (var application in shortlisted)
        {
            var interview = await _context.Interviews
                .FirstOrDefaultAsync(i => i.ApplicationId == application.Id && i.Status == InterviewStatus.Scheduled);

            if (interview == null)
            {
                interview = new Interview
                {
                    Id = Guid.NewGuid(),
                    ApplicationId = application.Id,
                    ScheduledAt = scheduledAt,
                    Mode = interviewMode,
                    MeetingLink = dto.MeetingLink,
                    Location = dto.Location,
                    Status = InterviewStatus.Scheduled,
                    Notes = FormatInterviewNotes(durationMinutes, dto.Notes),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Interviews.Add(interview);
            }
            else
            {
                interview.ScheduledAt = scheduledAt;
                interview.Mode = interviewMode;
                interview.MeetingLink = dto.MeetingLink;
                interview.Location = dto.Location;
                interview.Notes = FormatInterviewNotes(durationMinutes, dto.Notes);
                interview.UpdatedAt = DateTime.UtcNow;
            }

            interviewsScheduled++;

            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.StudentProfileId == application.StudentProfileId
                                       && c.CompanyProfileId == company.Id);

            if (conversation == null)
            {
                conversation = new Conversation
                {
                    Id = Guid.NewGuid(),
                    StudentProfileId = application.StudentProfileId,
                    CompanyProfileId = company.Id,
                    OpportunityId = opportunityId,
                    CreatedAt = DateTime.UtcNow,
                    LastMessageAt = DateTime.UtcNow
                };
                _context.Conversations.Add(conversation);
            }
            else if (conversation.OpportunityId == null)
            {
                conversation.OpportunityId = opportunityId;
            }

            var sentAt = DateTime.UtcNow;
            _context.Messages.Add(new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation.Id,
                SenderUserId = user.Id,
                Content = messageContent,
                IsRead = false,
                SentAt = sentAt
            });
            conversation.LastMessageAt = sentAt;
            messagesSent++;

            _context.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = application.StudentProfile.UserId,
                Type = NotificationType.Message,
                Title = "Interview invitation",
                Body = $"You have a new interview invitation for {opportunity.Title} at {companyName}.",
                RelatedOpportunityId = opportunityId,
                IsRead = false,
                CreatedAt = sentAt
            });
        }

        await _context.SaveChangesAsync();

        return new ScheduleInterviewsResultDto(
            opportunityId,
            opportunity.Title,
            shortlisted.Count,
            messagesSent,
            interviewsScheduled);
    }

    private async Task SendDeadlinePassedNotificationsAsync()
    {
        var todayUtc = DateTime.UtcNow.Date;

        var expired = await _context.Opportunities
            .Include(o => o.CompanyProfile)
            .Where(o => !o.DeadlineNotificationSent && o.DeadlineAt.Date < todayUtc)
            .ToListAsync();

        if (!expired.Any())
        {
            return;
        }

        foreach (var opportunity in expired)
        {
            var shortlistedCount = await _context.Applications
                .CountAsync(a => a.OpportunityId == opportunity.Id && a.Status == ApplicationStatus.Shortlisted);

            _context.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = opportunity.CompanyProfile.UserId,
                Type = NotificationType.Opportunity,
                Title = "Application deadline passed",
                Body = shortlistedCount > 0
                    ? $"The deadline for \"{opportunity.Title}\" has passed. You have {shortlistedCount} shortlisted candidate(s). Schedule interviews to notify them in chat."
                    : $"The deadline for \"{opportunity.Title}\" has passed. Schedule interviews when you have shortlisted candidates.",
                RelatedOpportunityId = opportunity.Id,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            opportunity.DeadlineNotificationSent = true;
            opportunity.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    private static string FormatInterviewNotes(int durationMinutes, string? notes)
    {
        var durationLine = $"Duration: {durationMinutes} minutes";
        return string.IsNullOrWhiteSpace(notes) ? durationLine : $"{durationLine}\n{notes.Trim()}";
    }

    private static string BuildInterviewChatMessage(
        string jobTitle,
        string companyName,
        DateTime scheduledAt,
        int durationMinutes,
        InterviewMode mode,
        string? meetingLink,
        string? location,
        string? notes)
    {
        var interviewData = new
        {
            role = jobTitle,
            company = companyName,
            date = scheduledAt.ToString("dddd, MMMM d, yyyy"),
            duration = $"{durationMinutes} minutes",
            format = mode.ToString(),
            meetingLink = meetingLink?.Trim(),
            location = location?.Trim(),
            notes = notes?.Trim()
        };

        return $"INTERVIEW_INVITATION::{System.Text.Json.JsonSerializer.Serialize(interviewData)}";
    }
}
