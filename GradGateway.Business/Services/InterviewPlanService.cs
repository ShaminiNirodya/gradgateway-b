using System.Globalization;
using System.Text.Json;
using GradGateway.Business.DTOs;
using GradGateway.Business.Helpers;
using GradGateway.Business.Interfaces;
using GradGateway.Data.Context;
using GradGateway.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GradGateway.Business.Services;

public class InterviewPlanService : IInterviewPlanService
{
    private readonly GradGatewayDbContext _context;
    private readonly IRealtimeNotificationService? _realtimeNotification;

    public InterviewPlanService(
        GradGatewayDbContext context,
        IRealtimeNotificationService? realtimeNotification = null)
    {
        _context = context;
        _realtimeNotification = realtimeNotification;
    }

    public async Task<OpportunityInterviewPlanDto?> GetPlanAsync(string firebaseUid, Guid opportunityId)
    {
        var company = await ResolveCompanyAsync(firebaseUid);
        var opportunity = await _context.Opportunities
            .FirstOrDefaultAsync(o => o.Id == opportunityId && o.CompanyProfileId == company.Id);
        if (opportunity == null)
        {
            return null;
        }

        var plan = await _context.OpportunityInterviewPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.OpportunityId == opportunityId);

        if (plan == null)
        {
            return null;
        }

        var shortlistedCount = await _context.Applications
            .CountAsync(a => a.OpportunityId == opportunityId && a.Status == ApplicationStatus.Shortlisted);

        return ToDto(plan, shortlistedCount);
    }

    public async Task<ScheduleInterviewsResultDto> SavePlanAsync(
        string firebaseUid,
        Guid opportunityId,
        ScheduleInterviewsRequestDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid)
                   ?? throw new InvalidOperationException("User not found.");

        if (user.Role != UserRole.Company)
        {
            throw new InvalidOperationException("Only company users can schedule interviews.");
        }

        var company = await ResolveCompanyAsync(firebaseUid);

        var opportunity = await _context.Opportunities
            .Include(o => o.CompanyProfile)
            .FirstOrDefaultAsync(o => o.Id == opportunityId && o.CompanyProfileId == company.Id)
            ?? throw new ArgumentException("Opportunity not found.");

        if (!Enum.TryParse<InterviewMode>(dto.Mode, true, out var interviewMode))
        {
            throw new ArgumentException("Invalid interview mode. Use Online, Onsite, or Phone.");
        }

        var rawDateStrings = dto.TentativeDates is { Count: > 0 }
            ? dto.TentativeDates
            : dto.ScheduledAt.HasValue
                ? new List<string> { dto.ScheduledAt.Value.ToString("yyyy-MM-dd") }
                : new List<string>();

        var tentativeDates = NormalizeTentativeDateStrings(rawDateStrings);
        var durationMinutes = dto.DurationMinutes is > 0 and <= 480 ? dto.DurationMinutes : 60;
        var primaryDate = tentativeDates[0];

        var plan = await _context.OpportunityInterviewPlans
            .FirstOrDefaultAsync(p => p.OpportunityId == opportunityId);

        var now = DateTime.UtcNow;
        if (plan == null)
        {
            plan = new OpportunityInterviewPlan
            {
                Id = Guid.NewGuid(),
                OpportunityId = opportunityId,
                CreatedAt = now,
            };
            _context.OpportunityInterviewPlans.Add(plan);
        }

        plan.TentativeDatesJson = JsonSerializer.Serialize(tentativeDates.Select(d => d.ToString("yyyy-MM-dd")).ToList());
        plan.DurationMinutes = durationMinutes;
        plan.Mode = interviewMode;
        plan.MeetingLink = dto.MeetingLink?.Trim();
        plan.Location = dto.Location?.Trim();
        plan.Notes = dto.Notes?.Trim();
        plan.UpdatedAt = now;

        var messagesSent = 0;
        var interviewsScheduled = 0;

        if (dto.NotifyExistingShortlisted)
        {
            var shortlisted = await _context.Applications
                .Include(a => a.StudentProfile)
                .Where(a => a.OpportunityId == opportunityId && a.Status == ApplicationStatus.Shortlisted)
                .ToListAsync();

            foreach (var application in shortlisted)
            {
                var sent = await SendPlanToApplicationAsync(
                    application,
                    opportunity,
                    company,
                    user.Id,
                    plan,
                    tentativeDates,
                    primaryDate,
                    durationMinutes,
                    interviewMode);
                if (sent)
                {
                    messagesSent++;
                    interviewsScheduled++;
                }
            }
        }

        await _context.SaveChangesAsync();

        var shortlistedCount = await _context.Applications
            .CountAsync(a => a.OpportunityId == opportunityId && a.Status == ApplicationStatus.Shortlisted);

        return new ScheduleInterviewsResultDto(
            opportunityId,
            opportunity.Title,
            shortlistedCount,
            messagesSent,
            interviewsScheduled,
            PlanSaved: true);
    }

    public async Task TryNotifyOnShortlistAsync(Guid applicationId, Guid companyUserId)
    {
        var application = await _context.Applications
            .Include(a => a.StudentProfile)
            .Include(a => a.Opportunity)
                .ThenInclude(o => o!.CompanyProfile)
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        if (application?.OpportunityId == null || application.Opportunity == null)
        {
            return;
        }

        if (application.Status != ApplicationStatus.Shortlisted)
        {
            return;
        }

        var plan = await _context.OpportunityInterviewPlans
            .FirstOrDefaultAsync(p => p.OpportunityId == application.OpportunityId);

        if (plan == null)
        {
            return;
        }

        if (application.InterviewPlanNotifiedAt.HasValue
            && application.InterviewPlanNotifiedAt.Value >= plan.UpdatedAt)
        {
            return;
        }

        var company = application.Opportunity.CompanyProfile;
        var tentativeDates = ParseTentativeDates(plan.TentativeDatesJson);
        if (tentativeDates.Count == 0)
        {
            return;
        }

        await SendPlanToApplicationAsync(
            application,
            application.Opportunity,
            company,
            companyUserId,
            plan,
            tentativeDates,
            tentativeDates[0],
            plan.DurationMinutes,
            plan.Mode);

        await _context.SaveChangesAsync();
    }

    private async Task<bool> SendPlanToApplicationAsync(
        Application application,
        Opportunity opportunity,
        CompanyProfile company,
        Guid senderUserId,
        OpportunityInterviewPlan plan,
        List<DateTime> tentativeDates,
        DateTime primaryDate,
        int durationMinutes,
        InterviewMode interviewMode)
    {
        if (application.InterviewPlanNotifiedAt.HasValue
            && application.InterviewPlanNotifiedAt.Value >= plan.UpdatedAt)
        {
            return false;
        }

        var interview = await _context.Interviews
            .FirstOrDefaultAsync(i => i.ApplicationId == application.Id && i.Status == InterviewStatus.Scheduled);

        var notesWithDates = FormatInterviewNotes(durationMinutes, plan.Notes, tentativeDates);

        if (interview == null)
        {
            interview = new Interview
            {
                Id = Guid.NewGuid(),
                ApplicationId = application.Id,
                ScheduledAt = DateTime.SpecifyKind(primaryDate.Date, DateTimeKind.Utc),
                Mode = interviewMode,
                MeetingLink = plan.MeetingLink,
                Location = plan.Location,
                Status = InterviewStatus.Scheduled,
                Notes = notesWithDates,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Interviews.Add(interview);
        }
        else
        {
            interview.ScheduledAt = DateTime.SpecifyKind(primaryDate.Date, DateTimeKind.Utc);
            interview.Mode = interviewMode;
            interview.MeetingLink = plan.MeetingLink;
            interview.Location = plan.Location;
            interview.Notes = notesWithDates;
            interview.UpdatedAt = DateTime.UtcNow;
        }

        var messageContent = BuildInterviewChatMessage(
            opportunity.Title,
            company.CompanyName,
            tentativeDates,
            durationMinutes,
            interviewMode,
            plan.MeetingLink,
            plan.Location,
            plan.Notes);

        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c =>
                c.StudentProfileId == application.StudentProfileId
                && c.CompanyProfileId == company.Id);

        if (conversation == null)
        {
            conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                StudentProfileId = application.StudentProfileId,
                CompanyProfileId = company.Id,
                OpportunityId = opportunity.Id,
                CreatedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow
            };
            _context.Conversations.Add(conversation);
        }
        else if (conversation.OpportunityId == null)
        {
            conversation.OpportunityId = opportunity.Id;
        }

        var sentAt = DateTime.UtcNow;
        _context.Messages.Add(new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderUserId = senderUserId,
            Content = messageContent,
            IsRead = false,
            SentAt = sentAt
        });
        conversation.LastMessageAt = sentAt;

        var interviewNotification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = application.StudentProfile.UserId,
            Type = NotificationType.Message,
            Title = "Interview invitation",
            Body = $"You have a new interview invitation for {opportunity.Title} at {company.CompanyName}.",
            RelatedOpportunityId = opportunity.Id,
            IsRead = false,
            CreatedAt = sentAt
        };
        _context.Notifications.Add(interviewNotification);

        application.InterviewPlanNotifiedAt = plan.UpdatedAt;

        await NotificationRealtimePublisher.PushAsync(
            _context,
            _realtimeNotification,
            application.StudentProfile.UserId,
            interviewNotification);

        return true;
    }

    private async Task<CompanyProfile> ResolveCompanyAsync(string firebaseUid)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid)
                   ?? throw new InvalidOperationException("User not found.");

        if (user.Role != UserRole.Company)
        {
            throw new InvalidOperationException("Only company users can access interview plans.");
        }

        return await _context.CompanyProfiles.FirstOrDefaultAsync(c => c.UserId == user.Id)
               ?? throw new InvalidOperationException("Company profile not found.");
    }

    private static List<DateTime> NormalizeTentativeDateStrings(IReadOnlyList<string> dateStrings)
    {
        if (dateStrings == null || dateStrings.Count == 0)
        {
            throw new ArgumentException("Add at least one interview date.");
        }

        var today = DateOnly.FromDateTime(DeadlineClock.TodayDateInSriLanka());
        var normalized = new List<DateTime>();
        var rejectedPast = new List<string>();

        foreach (var raw in dateStrings)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                continue;
            }

            // Accept yyyy-MM-dd only — avoids timezone shifts from ISO timestamps.
            if (!DateOnly.TryParseExact(
                    raw.Trim(),
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsed))
            {
                throw new ArgumentException($"Invalid date format: {raw}. Use the date picker (yyyy-MM-dd).");
            }

            if (parsed < today)
            {
                rejectedPast.Add(parsed.ToString("dd MMM yyyy"));
                continue;
            }

            normalized.Add(parsed.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        }

        normalized = normalized.Distinct().OrderBy(d => d).ToList();

        if (normalized.Count == 0)
        {
            if (rejectedPast.Count > 0)
            {
                throw new ArgumentException(
                    $"These interview dates are before today ({today:dd MMM yyyy}): {string.Join(", ", rejectedPast)}.");
            }

            throw new ArgumentException("Interview dates must be today or in the future.");
        }

        if (normalized.Count > 10)
        {
            throw new ArgumentException("You can add up to 10 tentative interview dates.");
        }

        return normalized;
    }

    private static List<DateTime> ParseTentativeDates(string json)
    {
        try
        {
            var strings = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            return strings
                .Select(s => DateTime.TryParse(s, out var d) ? DateTime.SpecifyKind(d.Date, DateTimeKind.Utc) : (DateTime?)null)
                .Where(d => d.HasValue)
                .Select(d => d!.Value)
                .OrderBy(d => d)
                .ToList();
        }
        catch
        {
            return new List<DateTime>();
        }
    }

    private static OpportunityInterviewPlanDto ToDto(OpportunityInterviewPlan plan, int shortlistedCount)
    {
        var dates = ParseTentativeDates(plan.TentativeDatesJson)
            .Select(d => d.ToString("yyyy-MM-dd"))
            .ToList();

        return new OpportunityInterviewPlanDto(
            plan.OpportunityId,
            dates,
            plan.DurationMinutes,
            plan.Mode.ToString(),
            plan.MeetingLink,
            plan.Location,
            plan.Notes,
            plan.UpdatedAt,
            shortlistedCount);
    }

    private static string FormatInterviewNotes(int durationMinutes, string? notes, List<DateTime> tentativeDates)
    {
        var dateLine = tentativeDates.Count > 1
            ? $"Tentative dates: {string.Join(", ", tentativeDates.Select(d => d.ToString("dd MMM yyyy")))}"
            : $"Date: {tentativeDates[0]:dd MMM yyyy}";
        var durationLine = $"Duration: {durationMinutes} minutes";
        var parts = new List<string> { dateLine, durationLine };
        if (!string.IsNullOrWhiteSpace(notes))
        {
            parts.Add(notes.Trim());
        }

        return string.Join("\n", parts);
    }

    public static string BuildInterviewChatMessage(
        string jobTitle,
        string companyName,
        List<DateTime> tentativeDates,
        int durationMinutes,
        InterviewMode mode,
        string? meetingLink,
        string? location,
        string? notes)
    {
        var formattedDates = tentativeDates
            .Select(d => d.ToString("dddd, MMMM d, yyyy"))
            .ToList();

        var interviewData = new
        {
            role = jobTitle,
            company = companyName,
            date = formattedDates[0],
            dates = formattedDates,
            multipleDates = formattedDates.Count > 1,
            duration = $"{durationMinutes} minutes",
            format = mode.ToString(),
            meetingLink = meetingLink?.Trim(),
            location = location?.Trim(),
            notes = notes?.Trim()
        };

        return $"INTERVIEW_INVITATION::{JsonSerializer.Serialize(interviewData)}";
    }
}
