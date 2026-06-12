using GradGateway.Business.DTOs;
using GradGateway.Business.Helpers;
using GradGateway.Business.Interfaces;
using GradGateway.Business;
using GradGateway.Data.Context;
using GradGateway.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GradGateway.Business.Services;

public class ApplicationService : IApplicationService
{
    private sealed record StudentThreadMessage(
        string Content,
        DateTime SentAt,
        Guid ConversationId,
        Guid CompanyProfileId);

    private sealed record ConversationContext(
        Guid Id,
        Guid CompanyProfileId,
        Guid? OpportunityId);

    private sealed record ConversationOffer(Guid? ApplicationId, string? Position);

    private readonly GradGatewayDbContext _context;
    private readonly IRealtimeNotificationService? _realtimeNotification;
    private readonly IInterviewPlanService _interviewPlanService;

    public ApplicationService(
        GradGatewayDbContext context,
        IInterviewPlanService interviewPlanService,
        IRealtimeNotificationService? realtimeNotification = null)
    {
        _context = context;
        _interviewPlanService = interviewPlanService;
        _realtimeNotification = realtimeNotification;
    }

    public async Task<ApplicationResponseDto> ApplyAsync(string firebaseUid, ApplyRequestDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
        if (user == null || user.Role != UserRole.Student)
            throw new InvalidOperationException("Only student users can apply.");

        var student = await _context.StudentProfiles.FirstOrDefaultAsync(s => s.UserId == user.Id);
        if (student == null)
            throw new InvalidOperationException("Student profile not found.");

        var todaySl = DeadlineClock.TodayDateInSriLanka();
        var opportunity = await _context.Opportunities
            .Include(o => o.CompanyProfile)
            .FirstOrDefaultAsync(o => o.Id == dto.OpportunityId);

        if (opportunity == null || !opportunity.IsActive)
            throw new ArgumentException("Opportunity not found.");

        if (DeadlineClock.IsDeadlinePassed(opportunity.DeadlineAt))
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
            RelatedOpportunityId = dto.OpportunityId,
            RelatedApplicationId = app.Id,
            RelatedStudentProfileId = student.Id,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.Notifications.Add(notif);

        await _context.SaveChangesAsync();
        await PushNotificationRealtimeAsync(opportunity.CompanyProfile.UserId, notif);

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
            app.UpdatedAt,
            app.CompanyProfileId
        );
    }

    public async Task<PagedResultDto<ApplicationResponseDto>> GetStudentApplicationsAsync(
        string firebaseUid,
        int page = 1,
        int pageSize = 50)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
        if (user == null || user.Role != UserRole.Student)
            throw new InvalidOperationException("Only student users can view this list.");

        var student = await _context.StudentProfiles.AsNoTracking().FirstOrDefaultAsync(s => s.UserId == user.Id);
        if (student == null)
            throw new InvalidOperationException("Student profile not found.");

        try
        {
            await SyncDirectOfferStatusesForStudentAsync(student);
        }
        catch
        {
            // Offer sync from chat is best-effort; still return applications.
        }

        var query = _context.Applications
            .AsNoTracking()
            .Include(a => a.Opportunity)
                .ThenInclude(o => o!.CompanyProfile)
            .Include(a => a.CompanyProfile)
            .Where(a => a.StudentProfileId == student.Id)
            .OrderByDescending(a => a.AppliedAt);

        var (normalizedPage, normalizedPageSize) = Pagination.Normalize(page, pageSize);
        var total = await query.CountAsync();
        var rows = await query
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToListAsync();

        var items = rows.Select(a =>
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
                a.UpdatedAt,
                a.CompanyProfileId);
        }).ToList();

        return new PagedResultDto<ApplicationResponseDto>(items, total, normalizedPage, normalizedPageSize);
    }

    public async Task<PagedResultDto<ApplicationResponseDto>> GetCompanyApplicationsAsync(
        string firebaseUid,
        int page = 1,
        int pageSize = 50)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
        if (user == null || user.Role != UserRole.Company)
            throw new InvalidOperationException("Only company users can view this list.");

        var company = await _context.CompanyProfiles.FirstOrDefaultAsync(c => c.UserId == user.Id);
        if (company == null)
            throw new InvalidOperationException("Company profile not found.");

        try
        {
            await SyncOfferRepliesForCompanyAsync(company.Id);
        }
        catch
        {
            // Offer sync from chat is best-effort; still return applications.
        }

        var query = _context.Applications
            .AsNoTracking()
            .Include(a => a.Opportunity)
            .Include(a => a.StudentProfile)
                .ThenInclude(s => s.User)
            .Where(a => (a.Opportunity != null && a.Opportunity.CompanyProfileId == company.Id) ||
                       (a.CompanyProfileId == company.Id))
            .OrderByDescending(a => a.AppliedAt);

        var (normalizedPage, normalizedPageSize) = Pagination.Normalize(page, pageSize);
        var total = await query.CountAsync();
        var rows = await query
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToListAsync();

        var items = rows.Select(a =>
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
                a.UpdatedAt,
                a.CompanyProfileId);
        }).ToList();

        return new PagedResultDto<ApplicationResponseDto>(items, total, normalizedPage, normalizedPageSize);
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

        if (!ApplicationStatusHelper.TryParse(status, out var parsed))
            throw new ArgumentException("Invalid application status.");

        var previousStatus = app.Status;
        var wasShortlisted = previousStatus == ApplicationStatus.Shortlisted;
        app.Status = parsed;
        app.UpdatedAt = DateTime.UtcNow;

        string jobTitle = app.Opportunity?.Title ?? app.JobTitle ?? "Direct Job Offer";
        string companyName = app.Opportunity?.CompanyProfile?.CompanyName ?? app.CompanyProfile?.CompanyName ?? company.CompanyName;

        Message? outcomeChatMessage = null;
        Notification? statusNotification = null;

        if (parsed == ApplicationStatus.Hired && previousStatus != ApplicationStatus.Hired)
        {
            (outcomeChatMessage, statusNotification) = await PrepareApplicationOutcomeAsync(
                app,
                company,
                user,
                companyName,
                jobTitle,
                hired: true);
        }
        else if (parsed == ApplicationStatus.Rejected
                 && previousStatus != ApplicationStatus.Rejected
                 && ApplicationOutcomeMessages.ShouldSendRejectionNotice(previousStatus))
        {
            (outcomeChatMessage, statusNotification) = await PrepareApplicationOutcomeAsync(
                app,
                company,
                user,
                companyName,
                jobTitle,
                hired: false);
        }
        else
        {
            statusNotification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = app.StudentProfile.UserId,
                Type = NotificationType.Application,
                Title = "Application Status Updated",
                Body = $"Your application for {jobTitle} is now {parsed}.",
                RelatedOpportunityId = app.OpportunityId,
                RelatedApplicationId = app.Id,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(statusNotification);
        }

        await _context.SaveChangesAsync();

        if (statusNotification != null)
        {
            await NotificationRealtimePublisher.PushAsync(
                _context,
                _realtimeNotification,
                app.StudentProfile.UserId,
                statusNotification);
        }

        if (outcomeChatMessage != null)
        {
            await PushOutcomeChatMessageRealtimeAsync(
                app.StudentProfile.UserId,
                user,
                outcomeChatMessage);
        }

        if (parsed == ApplicationStatus.Shortlisted && !wasShortlisted && app.OpportunityId.HasValue)
        {
            await _interviewPlanService.TryNotifyOnShortlistAsync(app.Id, user.Id);
        }

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
            app.UpdatedAt,
            app.CompanyProfileId
        );
    }

    public async Task<ApplicationResponseDto> CreateJobOfferApplicationAsync(
        string firebaseUid,
        Guid studentProfileId,
        string jobTitle,
        string jobType,
        string? compensation,
        string proposalMessage,
        Guid? opportunityId = null)
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

        Application app;

        if (opportunityId.HasValue)
        {
            var opportunity = await _context.Opportunities
                .FirstOrDefaultAsync(o => o.Id == opportunityId.Value);
            if (opportunity == null)
                throw new ArgumentException("Job post not found.");
            if (opportunity.CompanyProfileId != company.Id)
                throw new InvalidOperationException("You can only share your own job posts as offers.");

            var existing = await _context.Applications
                .FirstOrDefaultAsync(a =>
                    a.OpportunityId == opportunityId.Value &&
                    a.StudentProfileId == student.Id);

            if (existing != null)
            {
                existing.Status = ApplicationStatus.OfferSent;
                existing.CompanyProfileId ??= company.Id;
                existing.JobTitle = jobTitle;
                existing.JobType = jobType;
                existing.Compensation = compensation;
                existing.CoverLetter = proposalMessage;
                existing.UpdatedAt = DateTime.UtcNow;
                app = existing;
            }
            else
            {
                app = new Application
                {
                    Id = Guid.NewGuid(),
                    OpportunityId = opportunityId.Value,
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
            }
        }
        else
        {
            var existingDirect = await FindPendingDirectOfferAsync(student.Id, company.Id);
            if (existingDirect != null
                && string.Equals(existingDirect.JobTitle, jobTitle, StringComparison.OrdinalIgnoreCase))
            {
                existingDirect.CoverLetter = proposalMessage;
                existingDirect.JobType = jobType;
                existingDirect.Compensation = compensation;
                existingDirect.UpdatedAt = DateTime.UtcNow;
                app = existingDirect;
            }
            else
            {
                app = new Application
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
            }
        }

        var notif = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = student.UserId,
            Type = NotificationType.Application,
            Title = "Job Offer Received",
            Body = $"{company.CompanyName} sent you a job offer for {jobTitle}.",
            RelatedOpportunityId = app.OpportunityId,
            RelatedApplicationId = app.Id,
            RelatedStudentProfileId = student.Id,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.Notifications.Add(notif);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException(
                "Could not save the job offer. The database may need the direct job offer migration applied.",
                ex);
        }

        await NotificationRealtimePublisher.PushAsync(_context, _realtimeNotification, student.UserId, notif);

        var displayTitle = app.JobTitle ?? jobTitle;
        return new ApplicationResponseDto(
            app.Id,
            app.OpportunityId,
            student.Id,
            displayTitle,
            company.CompanyName,
            student.FullName,
            student.User.Email,
            app.CoverLetter,
            app.Status.ToString(),
            app.AppliedAt,
            app.UpdatedAt,
            app.CompanyProfileId
        );
    }

    public async Task<Notification?> TryApplyJobOfferResponseInConversationAsync(
        string firebaseUid,
        Guid conversationId,
        bool accepted,
        Guid? applicationId = null)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
        if (user == null || user.Role != UserRole.Student)
            return null;

        var student = await _context.StudentProfiles.FirstOrDefaultAsync(s => s.UserId == user.Id);
        if (student == null)
            return null;

        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == conversationId && c.StudentProfileId == student.Id);
        if (conversation == null)
            return null;

        Application? app = await FindPendingOfferForConversationAsync(
            student.Id,
            conversationId,
            conversation.CompanyProfileId,
            applicationId);

        if (app == null)
        {
            var latestReply = await _context.Messages
                .Where(m => m.ConversationId == conversationId && m.SenderUserId == user.Id)
                .OrderByDescending(m => m.SentAt)
                .Select(m => m.Content)
                .FirstOrDefaultAsync();

            if (JobOfferResponseMatcher.TryParse(latestReply, out var fromMessage))
            {
                accepted = fromMessage;
                app = await FindPendingOfferForConversationAsync(
                    student.Id,
                    conversationId,
                    conversation.CompanyProfileId,
                    applicationId);
            }
        }

        if (app == null || app.CompanyProfile == null)
            return null;

        app.Status = accepted ? ApplicationStatus.OfferAccepted : ApplicationStatus.Rejected;
        app.UpdatedAt = DateTime.UtcNow;

        var jobTitle = app.JobTitle ?? "Job Offer";

        var notif = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = app.CompanyProfile.UserId,
            Type = NotificationType.Application,
            Title = accepted ? "Offer accepted" : "Offer declined",
            Body = accepted
                ? $"{student.FullName} is open for an interview for {jobTitle}."
                : $"{student.FullName} declined the offer for {jobTitle}.",
            RelatedOpportunityId = app.OpportunityId,
            RelatedApplicationId = app.Id,
            RelatedConversationId = conversationId,
            RelatedStudentProfileId = student.Id,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.Notifications.Add(notif);

        return notif;
    }

    public async Task<ApplicationResponseDto> RespondToJobOfferAsync(
        string firebaseUid,
        Guid conversationId,
        bool accepted,
        Guid? applicationId = null)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
        if (user == null || user.Role != UserRole.Student)
            throw new InvalidOperationException("Only student users can respond to job offers.");

        var student = await _context.StudentProfiles
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == user.Id);
        if (student == null)
            throw new InvalidOperationException("Student profile not found.");

        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == conversationId && c.StudentProfileId == student.Id);
        if (conversation == null)
            throw new ArgumentException("Conversation not found.");

        var notif = await TryApplyJobOfferResponseInConversationAsync(
            firebaseUid,
            conversationId,
            accepted,
            applicationId);
        if (notif == null)
        {
            await SyncDirectOfferStatusesForStudentAsync(student);
            notif = await TryApplyJobOfferResponseInConversationAsync(
                firebaseUid,
                conversationId,
                accepted,
                applicationId);
        }

        var app = applicationId.HasValue
            ? await _context.Applications
                .Include(a => a.CompanyProfile)
                .Include(a => a.StudentProfile)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(a =>
                    a.Id == applicationId.Value && a.StudentProfileId == student.Id)
            : await FindOfferApplicationForConversationAsync(
                student.Id,
                conversationId,
                conversation.CompanyProfileId);

        if (app == null)
            throw new InvalidOperationException("No pending job offer found for this conversation.");

        if (notif == null)
        {
            if (app.Status == ApplicationStatus.OfferSent)
                throw new InvalidOperationException("No pending job offer found for this conversation.");

            return new ApplicationResponseDto(
                app.Id,
                app.OpportunityId,
                student.Id,
                app.JobTitle ?? "Job Offer",
                app.CompanyProfile?.CompanyName ?? "Company",
                student.FullName,
                student.User.Email,
                app.CoverLetter,
                app.Status.ToString(),
                app.AppliedAt,
                app.UpdatedAt,
                app.CompanyProfileId);
        }

        await _context.SaveChangesAsync();
        await NotificationRealtimePublisher.PushAsync(_context, _realtimeNotification, notif.UserId, notif);

        var jobTitle = app.JobTitle ?? "Job Offer";
        var companyName = app.CompanyProfile?.CompanyName ?? "Company";

        return new ApplicationResponseDto(
            app.Id,
            app.OpportunityId,
            student.Id,
            jobTitle,
            companyName,
            student.FullName,
            student.User.Email,
            app.CoverLetter,
            app.Status.ToString(),
            app.AppliedAt,
            app.UpdatedAt,
            app.CompanyProfileId
        );
    }

    private Task PushNotificationRealtimeAsync(Guid userId, Notification notification) =>
        NotificationRealtimePublisher.PushAsync(_context, _realtimeNotification, userId, notification);

    /// <summary>
    /// Applies offer accept/decline from chat history when the student already replied but status was not updated.
    /// </summary>
    private Task<Application?> FindPendingDirectOfferAsync(Guid studentProfileId, Guid companyProfileId) =>
        _context.Applications
            .Include(a => a.CompanyProfile)
            .Include(a => a.StudentProfile)
                .ThenInclude(s => s.User)
            .Where(a =>
                a.StudentProfileId == studentProfileId &&
                a.CompanyProfileId == companyProfileId &&
                a.OpportunityId == null &&
                a.Status == ApplicationStatus.OfferSent)
            .OrderByDescending(a => a.AppliedAt)
            .FirstOrDefaultAsync();

    public async Task<int> SyncDirectOfferStatusesFromMessagesAsync(string firebaseUid)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            if (user == null || user.Role != UserRole.Student)
                return 0;

            var student = await _context.StudentProfiles.FirstOrDefaultAsync(s => s.UserId == user.Id);
            if (student == null)
                return 0;

            return await SyncDirectOfferStatusesForStudentAsync(student);
        }
        catch
        {
            return 0;
        }
    }

    private async Task SyncOfferRepliesForCompanyAsync(Guid companyProfileId)
    {
        var studentIds = await _context.Applications
            .Where(a =>
                a.Status == ApplicationStatus.OfferSent &&
                (a.CompanyProfileId == companyProfileId ||
                 (a.OpportunityId != null &&
                  _context.Opportunities.Any(o =>
                      o.Id == a.OpportunityId && o.CompanyProfileId == companyProfileId))))
            .Select(a => a.StudentProfileId)
            .Distinct()
            .ToListAsync();

        foreach (var studentId in studentIds)
        {
            var student = await _context.StudentProfiles.FirstOrDefaultAsync(s => s.Id == studentId);
            if (student != null)
                await SyncDirectOfferStatusesForStudentAsync(student);
        }
    }

    private async Task<int> SyncDirectOfferStatusesForStudentAsync(StudentProfile student)
    {
        var userId = student.UserId;
        var pendingOffers = await _context.Applications
            .Include(a => a.CompanyProfile)
            .Include(a => a.Opportunity)
            .Where(a =>
                a.StudentProfileId == student.Id &&
                a.Status == ApplicationStatus.OfferSent)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();

        if (pendingOffers.Count == 0)
            return 0;

        var conversations = await _context.Conversations
            .Where(c => c.StudentProfileId == student.Id)
            .Select(c => new ConversationContext(c.Id, c.CompanyProfileId, c.OpportunityId))
            .ToListAsync();

        var conversationIds = conversations.Select(c => c.Id).ToList();
        var conversationById = conversations.ToDictionary(c => c.Id);

        List<StudentThreadMessage> studentMessages;
        List<(Guid ConversationId, string Content)> offerMessageRows;

        if (conversationIds.Count == 0)
        {
            studentMessages = new List<StudentThreadMessage>();
            offerMessageRows = new List<(Guid ConversationId, string Content)>();
        }
        else
        {
            studentMessages = await _context.Messages
                .Where(m => m.SenderUserId == userId && conversationIds.Contains(m.ConversationId))
                .Join(
                    _context.Conversations,
                    m => m.ConversationId,
                    c => c.Id,
                    (m, c) => new StudentThreadMessage(m.Content, m.SentAt, m.ConversationId, c.CompanyProfileId))
                .OrderByDescending(x => x.SentAt)
                .ToListAsync();

            var offerMessages = await _context.Messages
                .Where(m => conversationIds.Contains(m.ConversationId)
                            && m.Content.StartsWith(JobOfferMessageHelper.Prefix))
                .OrderByDescending(m => m.SentAt)
                .Select(m => new { m.ConversationId, m.Content })
                .ToListAsync();

            offerMessageRows = offerMessages
                .Select(o => (o.ConversationId, o.Content))
                .ToList();
        }

        var conversationOffers = BuildConversationOffersIndex(offerMessageRows);

        var updated = 0;

        foreach (var app in pendingOffers)
        {
            if (!TryResolveOfferReplyForApplication(
                    app,
                    studentMessages,
                    conversationOffers,
                    conversationById,
                    out var accepted))
                continue;

            app.Status = accepted ? ApplicationStatus.OfferAccepted : ApplicationStatus.Rejected;
            app.UpdatedAt = DateTime.UtcNow;
            updated++;
        }

        if (updated > 0)
            await _context.SaveChangesAsync();

        return updated;
    }

    private static Dictionary<Guid, List<ConversationOffer>> BuildConversationOffersIndex(
        IEnumerable<(Guid ConversationId, string Content)> offerMessages)
    {
        var conversationOffers = new Dictionary<Guid, List<ConversationOffer>>();

        foreach (var (conversationId, content) in offerMessages)
        {

            if (!conversationOffers.TryGetValue(conversationId, out var list))
            {
                list = new List<ConversationOffer>();
                conversationOffers[conversationId] = list;
            }

            Guid? applicationId = JobOfferMessageHelper.TryGetApplicationId(content, out var parsedId)
                ? parsedId
                : null;
            string? position = JobOfferMessageHelper.TryGetPosition(content, out var parsedPosition)
                ? parsedPosition
                : null;

            list.Add(new ConversationOffer(applicationId, position));
        }

        return conversationOffers;
    }

    private static bool JobTitlesMatch(string? offerPosition, Application app)
    {
        if (string.IsNullOrWhiteSpace(offerPosition))
            return false;

        var appTitle = app.JobTitle?.Trim();
        if (string.IsNullOrEmpty(appTitle))
            appTitle = app.Opportunity?.Title?.Trim();

        return !string.IsNullOrEmpty(appTitle)
               && string.Equals(offerPosition.Trim(), appTitle, StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryResolveOfferReplyForApplication(
        Application app,
        IReadOnlyList<StudentThreadMessage> studentMessages,
        IReadOnlyDictionary<Guid, List<ConversationOffer>> conversationOffers,
        IReadOnlyDictionary<Guid, ConversationContext> conversationById,
        out bool accepted)
    {
        accepted = false;

        foreach (var message in studentMessages)
        {
            if (!JobOfferResponseMatcher.TryParse(message.Content, out var replyAccepted))
                continue;

            if (conversationOffers.TryGetValue(message.ConversationId, out var offers) && offers.Count > 0)
            {
                foreach (var offer in offers)
                {
                    if (offer.ApplicationId.HasValue && offer.ApplicationId.Value == app.Id)
                    {
                        accepted = replyAccepted;
                        return true;
                    }

                    if (JobTitlesMatch(offer.Position, app))
                    {
                        accepted = replyAccepted;
                        return true;
                    }
                }

                continue;
            }

            if (!conversationById.TryGetValue(message.ConversationId, out var conversation))
                continue;

            if (app.OpportunityId.HasValue
                && conversation.OpportunityId.HasValue
                && app.OpportunityId.Value == conversation.OpportunityId.Value)
            {
                accepted = replyAccepted;
                return true;
            }

            if (app.OpportunityId == null
                && !conversation.OpportunityId.HasValue
                && app.CompanyProfileId.HasValue
                && app.CompanyProfileId.Value == message.CompanyProfileId)
            {
                accepted = replyAccepted;
                return true;
            }
        }

        return false;
    }

    private async Task<Application?> FindPendingOfferForConversationAsync(
        Guid studentProfileId,
        Guid conversationId,
        Guid companyProfileId,
        Guid? applicationId = null) =>
        await FindOfferApplicationForConversationAsync(
            studentProfileId,
            conversationId,
            companyProfileId,
            ApplicationStatus.OfferSent,
            applicationId);

    private async Task<Application?> FindOfferApplicationForConversationAsync(
        Guid studentProfileId,
        Guid conversationId,
        Guid companyProfileId,
        ApplicationStatus? requiredStatus = null,
        Guid? preferredApplicationId = null)
    {
        if (preferredApplicationId.HasValue)
        {
            IQueryable<Application> preferredQuery = _context.Applications
                .Include(a => a.CompanyProfile)
                .Include(a => a.StudentProfile)
                    .ThenInclude(s => s.User)
                .Where(a =>
                    a.Id == preferredApplicationId.Value &&
                    a.StudentProfileId == studentProfileId);

            if (requiredStatus.HasValue)
                preferredQuery = preferredQuery.Where(a => a.Status == requiredStatus.Value);

            var preferred = await preferredQuery.FirstOrDefaultAsync();
            if (preferred != null)
                return preferred;
        }

        if (await TryGetLinkedApplicationIdFromConversationAsync(conversationId) is Guid linkedId)
        {
            IQueryable<Application> byIdQuery = _context.Applications
                .Include(a => a.CompanyProfile)
                .Include(a => a.StudentProfile)
                    .ThenInclude(s => s.User)
                .Where(a => a.Id == linkedId && a.StudentProfileId == studentProfileId);

            if (requiredStatus.HasValue)
                byIdQuery = byIdQuery.Where(a => a.Status == requiredStatus.Value);

            var byId = await byIdQuery.FirstOrDefaultAsync();
            if (byId != null)
                return byId;
        }

        var conversation = await _context.Conversations.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conversation?.OpportunityId != null)
        {
            IQueryable<Application> byOpportunityQuery = _context.Applications
                .Include(a => a.CompanyProfile)
                .Include(a => a.StudentProfile)
                    .ThenInclude(s => s.User)
                .Where(a =>
                    a.StudentProfileId == studentProfileId &&
                    a.OpportunityId == conversation.OpportunityId);

            if (requiredStatus.HasValue)
                byOpportunityQuery = byOpportunityQuery.Where(a => a.Status == requiredStatus.Value);

            var byOpportunity = await byOpportunityQuery
                .OrderByDescending(a => a.AppliedAt)
                .FirstOrDefaultAsync();

            if (byOpportunity != null)
                return byOpportunity;
        }

        var offerContents = await _context.Messages
            .Where(m => m.ConversationId == conversationId && m.Content.StartsWith(JobOfferMessageHelper.Prefix))
            .OrderByDescending(m => m.SentAt)
            .Select(m => m.Content)
            .ToListAsync();

        foreach (var offerContent in offerContents)
        {
            if (!JobOfferMessageHelper.TryGetPosition(offerContent, out var position))
                continue;

            IQueryable<Application> byTitleQuery = _context.Applications
                .Include(a => a.CompanyProfile)
                .Include(a => a.StudentProfile)
                    .ThenInclude(s => s.User)
                .Where(a =>
                    a.StudentProfileId == studentProfileId &&
                    a.CompanyProfileId == companyProfileId &&
                    a.JobTitle == position);

            if (requiredStatus.HasValue)
                byTitleQuery = byTitleQuery.Where(a => a.Status == requiredStatus.Value);

            var byTitle = await byTitleQuery
                .OrderByDescending(a => a.AppliedAt)
                .FirstOrDefaultAsync();

            if (byTitle != null)
                return byTitle;
        }

        if (!requiredStatus.HasValue || requiredStatus == ApplicationStatus.OfferSent)
        {
            return await FindPendingDirectOfferAsync(studentProfileId, companyProfileId);
        }

        var fallbackQuery = _context.Applications
            .Include(a => a.CompanyProfile)
            .Include(a => a.StudentProfile)
                .ThenInclude(s => s.User)
            .Where(a =>
                a.StudentProfileId == studentProfileId &&
                a.CompanyProfileId == companyProfileId);

        if (requiredStatus.HasValue)
            fallbackQuery = fallbackQuery.Where(a => a.Status == requiredStatus.Value);

        return await fallbackQuery
            .OrderByDescending(a => a.UpdatedAt)
            .FirstOrDefaultAsync();
    }

    private async Task<Guid?> TryGetLinkedApplicationIdFromConversationAsync(Guid conversationId)
    {
        var offerContents = await _context.Messages
            .Where(m => m.ConversationId == conversationId && m.Content.StartsWith(JobOfferMessageHelper.Prefix))
            .OrderByDescending(m => m.SentAt)
            .Select(m => m.Content)
            .ToListAsync();

        foreach (var content in offerContents)
        {
            if (JobOfferMessageHelper.TryGetApplicationId(content, out var applicationId))
                return applicationId;
        }

        return null;
    }

    private async Task<(Message Message, Notification Notification)> PrepareApplicationOutcomeAsync(
        Application app,
        CompanyProfile company,
        User companyUser,
        string companyName,
        string jobTitle,
        bool hired)
    {
        var student = app.StudentProfile;
        var content = hired
            ? ApplicationOutcomeMessages.BuildHiredMessage(student.FullName, jobTitle, companyName)
            : ApplicationOutcomeMessages.BuildRejectedMessage(student.FullName, jobTitle, companyName);

        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c =>
                c.StudentProfileId == app.StudentProfileId &&
                c.CompanyProfileId == company.Id);

        if (conversation == null)
        {
            conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                StudentProfileId = app.StudentProfileId,
                CompanyProfileId = company.Id,
                OpportunityId = app.OpportunityId,
                CreatedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow
            };
            _context.Conversations.Add(conversation);
        }
        else if (conversation.OpportunityId == null && app.OpportunityId != null)
        {
            conversation.OpportunityId = app.OpportunityId;
        }

        var sentAt = DateTime.UtcNow;
        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderUserId = companyUser.Id,
            Content = content,
            IsRead = false,
            SentAt = sentAt
        };
        _context.Messages.Add(message);
        conversation.LastMessageAt = sentAt;

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = student.UserId,
            Type = hired ? NotificationType.Application : NotificationType.Message,
            Title = hired ? "Congratulations — you're hired!" : "Application update",
            Body = hired
                ? $"{companyName} hired you for {jobTitle}. Open Messages to read their note."
                : $"{companyName} shared an update about your application for {jobTitle}. Open Messages to read their note.",
            RelatedOpportunityId = app.OpportunityId,
            RelatedApplicationId = app.Id,
            RelatedConversationId = conversation.Id,
            IsRead = false,
            CreatedAt = sentAt
        };
        _context.Notifications.Add(notification);

        return (message, notification);
    }

    private async Task PushOutcomeChatMessageRealtimeAsync(
        Guid recipientUserId,
        User sender,
        Message message)
    {
        if (_realtimeNotification == null)
            return;

        try
        {
            var response = new MessageResponseDto(
                message.Id,
                message.ConversationId,
                message.SenderUserId,
                sender.Email,
                message.Content,
                message.IsRead,
                message.SentAt,
                message.AttachmentUrl,
                message.AttachmentName,
                message.AttachmentType);

            await _realtimeNotification.NotifyNewMessageAsync(recipientUserId, response);
        }
        catch
        {
            // Non-blocking — student can still read the message in inbox.
        }
    }

    public async Task<CompanyAnalyticsDto> GetCompanyAnalyticsAsync(string firebaseUid)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid)
            ?? throw new InvalidOperationException("User not found.");
        if (user.Role != UserRole.Company)
            throw new InvalidOperationException("Only company users can view analytics.");

        var company = await _context.CompanyProfiles.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == user.Id)
            ?? throw new InvalidOperationException("Company profile not found.");

        var rows = await _context.Applications
            .AsNoTracking()
            .Where(a =>
                (a.Opportunity != null && a.Opportunity.CompanyProfileId == company.Id) ||
                a.CompanyProfileId == company.Id)
            .Select(a => new { a.Status, a.AppliedAt })
            .ToListAsync();

        var total = rows.Count;
        var shortlisted = rows.Count(r => r.Status == ApplicationStatus.Shortlisted);
        var interviewed = rows.Count(r => r.Status == ApplicationStatus.Interviewed);
        var offersSent = rows.Count(r => r.Status == ApplicationStatus.OfferSent || r.Status == ApplicationStatus.OfferAccepted);
        var hired = rows.Count(r => r.Status == ApplicationStatus.Hired);

        var today = DateTime.UtcNow.Date;
        var byDay = Enumerable.Range(0, 30)
            .Select(i => today.AddDays(-29 + i))
            .Select(d => new AnalyticsDataPointDto(
                d.ToString("MMM d"),
                rows.Count(r => r.AppliedAt.Date == d),
                d))
            .ToList();

        var weekStart = today.AddDays(-((int)today.DayOfWeek + 6) % 7);
        var byWeek = Enumerable.Range(0, 12)
            .Select(i => weekStart.AddDays(-7 * (11 - i)))
            .Select(week =>
            {
                var end = week.AddDays(7);
                return new AnalyticsDataPointDto(
                    week.ToString("MMM d"),
                    rows.Count(r => r.AppliedAt.Date >= week && r.AppliedAt.Date < end),
                    week);
            })
            .ToList();

        return new CompanyAnalyticsDto(total, shortlisted, interviewed, offersSent, hired, byDay, byWeek);
    }
}
