using GradGateway.Business;
using GradGateway.Business.DTOs;
using GradGateway.Business.Interfaces;
using GradGateway.Data.Context;
using GradGateway.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GradGateway.Business.Services;

public class ConversationService : IConversationService
{
    private readonly GradGatewayDbContext _context;
    private readonly IApplicationService _applicationService;
    private readonly IRealtimeNotificationService? _realtimeNotification;

    public ConversationService(
        GradGatewayDbContext context,
        IApplicationService applicationService,
        IRealtimeNotificationService? realtimeNotification = null)
    {
        _context = context;
        _applicationService = applicationService;
        _realtimeNotification = realtimeNotification;
    }

    public async Task<ConversationResponseDto> StartConversationAsync(string firebaseUid, StartConversationRequestDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid)
                   ?? throw new InvalidOperationException("User not found.");

        Guid studentProfileId;
        Guid companyProfileId;

        if (user.Role == UserRole.Student)
        {
            var student = await _context.StudentProfiles.FirstOrDefaultAsync(s => s.UserId == user.Id)
                          ?? throw new InvalidOperationException("Student profile not found.");

            if (dto.OpportunityId == null)
                throw new ArgumentException("OpportunityId is required for student chat start.");

            var opp = await _context.Opportunities.Include(o => o.CompanyProfile)
                .FirstOrDefaultAsync(o => o.Id == dto.OpportunityId.Value)
                ?? throw new ArgumentException("Opportunity not found.");

            studentProfileId = student.Id;
            companyProfileId = opp.CompanyProfileId;
        }
        else if (user.Role == UserRole.Company)
        {
            var company = await _context.CompanyProfiles.FirstOrDefaultAsync(c => c.UserId == user.Id)
                          ?? throw new InvalidOperationException("Company profile not found.");

            if (dto.StudentProfileId == null)
                throw new ArgumentException("StudentProfileId is required for company chat start.");

            var student = await _context.StudentProfiles.FirstOrDefaultAsync(s => s.Id == dto.StudentProfileId.Value)
                          ?? throw new ArgumentException("Student profile not found.");

            studentProfileId = student.Id;
            companyProfileId = company.Id;
        }
        else
        {
            throw new InvalidOperationException("Unsupported role for conversations.");
        }

        // Keep one canonical chat thread per student-company pair.
        var existing = await _context.Conversations
            .FirstOrDefaultAsync(c => c.StudentProfileId == studentProfileId
                                   && c.CompanyProfileId == companyProfileId);

        var convo = existing;
        if (convo == null)
        {
            convo = new Conversation
            {
                Id = Guid.NewGuid(),
                StudentProfileId = studentProfileId,
                CompanyProfileId = companyProfileId,
                OpportunityId = dto.OpportunityId,
                CreatedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow
            };
            _context.Conversations.Add(convo);
            await _context.SaveChangesAsync();
        }
        else if (convo.OpportunityId == null && dto.OpportunityId != null)
        {
            // Backfill opportunity reference when the existing thread was created from talent search.
            convo.OpportunityId = dto.OpportunityId;
            await _context.SaveChangesAsync();
        }

        var studentName = await _context.StudentProfiles.Where(s => s.Id == convo.StudentProfileId).Select(s => s.FullName).FirstAsync();
        var studentPhoto = await _context.StudentProfiles.Where(s => s.Id == convo.StudentProfileId).Select(s => s.PhotoDataUrl).FirstOrDefaultAsync();
        var companyName = await _context.CompanyProfiles.Where(c => c.Id == convo.CompanyProfileId).Select(c => c.CompanyName).FirstAsync();
        var companyLogo = await _context.CompanyProfiles.Where(c => c.Id == convo.CompanyProfileId).Select(c => c.LogoDataUrl).FirstOrDefaultAsync();
        var other = user.Role == UserRole.Student ? companyName : studentName;
        var otherPhoto = user.Role == UserRole.Student ? companyLogo : studentPhoto;

        return new ConversationResponseDto(convo.Id, convo.OpportunityId, other, otherPhoto, string.Empty, convo.LastMessageAt, false);
    }

    public async Task<List<ConversationResponseDto>> GetMyConversationsAsync(string firebaseUid)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid)
                   ?? throw new InvalidOperationException("User not found.");

        await CollapseStaleMessageNotificationsAsync(user.Id);

        IQueryable<Conversation> query = _context.Conversations
            .Include(c => c.StudentProfile)
            .Include(c => c.CompanyProfile)
            .Include(c => c.Opportunity)
            .OrderByDescending(c => c.LastMessageAt);

        if (user.Role == UserRole.Student)
        {
            var student = await _context.StudentProfiles.FirstOrDefaultAsync(s => s.UserId == user.Id)
                          ?? throw new InvalidOperationException("Student profile not found.");
            query = query.Where(c => c.StudentProfileId == student.Id);
        }
        else if (user.Role == UserRole.Company)
        {
            var company = await _context.CompanyProfiles.FirstOrDefaultAsync(c => c.UserId == user.Id)
                          ?? throw new InvalidOperationException("Company profile not found.");
            query = query.Where(c => c.CompanyProfileId == company.Id);
        }
        else
        {
            query = query.Where(c => false);
        }

        var rows = await query.ToListAsync();
        var conversationIds = rows.Select(r => r.Id).ToList();
        var lastMessages = await _context.Messages
            .Where(m => conversationIds.Contains(m.ConversationId))
            .GroupBy(m => m.ConversationId)
            .Select(g => g.OrderByDescending(x => x.SentAt).First())
            .ToListAsync();

        var lastMessageByConversationId = lastMessages.ToDictionary(x => x.ConversationId, x => x);

        var unreadConversationIds = await _context.Messages
            .Where(m => conversationIds.Contains(m.ConversationId)
                        && !m.IsRead
                        && m.SenderUserId != user.Id)
            .Select(m => m.ConversationId)
            .Distinct()
            .ToListAsync();
        var unreadSet = unreadConversationIds.ToHashSet();

        // Merge legacy duplicate threads per student-company pair (unread + latest message across all thread ids).
        var aggregated = rows
            .GroupBy(c => new { c.StudentProfileId, c.CompanyProfileId })
            .Select(group =>
            {
                var threadIds = group.Select(c => c.Id).ToList();
                var hasUnread = threadIds.Any(id => unreadSet.Contains(id));

                Message? latestMessage = null;
                foreach (var threadId in threadIds)
                {
                    if (!lastMessageByConversationId.TryGetValue(threadId, out var candidate))
                    {
                        continue;
                    }

                    if (latestMessage == null || candidate.SentAt > latestMessage.SentAt)
                    {
                        latestMessage = candidate;
                    }
                }

                if (latestMessage != null
                    && latestMessage.SenderUserId != user.Id
                    && !latestMessage.IsRead)
                {
                    hasUnread = true;
                }

                var canonical = group
                    .OrderByDescending(c => lastMessageByConversationId.ContainsKey(c.Id))
                    .ThenByDescending(c => lastMessageByConversationId.TryGetValue(c.Id, out var lm) ? lm.SentAt : DateTime.MinValue)
                    .ThenByDescending(c => c.LastMessageAt)
                    .First();

                var displayConversation = latestMessage != null
                    ? group.FirstOrDefault(c => c.Id == latestMessage.ConversationId) ?? canonical
                    : canonical;

                return new
                {
                    Conversation = displayConversation,
                    HasUnread = hasUnread,
                    LatestMessage = latestMessage,
                };
            })
            .OrderByDescending(x => x.HasUnread)
            .ThenByDescending(x => x.LatestMessage?.SentAt ?? x.Conversation.LastMessageAt)
            .ToList();

        return aggregated.Select(entry =>
        {
            var c = entry.Conversation;
            var lm = entry.LatestMessage;
            var other = user.Role == UserRole.Student ? c.CompanyProfile.CompanyName : c.StudentProfile.FullName;
            var otherPhoto = user.Role == UserRole.Student ? c.CompanyProfile.LogoDataUrl : c.StudentProfile.PhotoDataUrl;
            return new ConversationResponseDto(
                c.Id,
                c.OpportunityId,
                other,
                otherPhoto,
                lm?.Content ?? string.Empty,
                lm?.SentAt ?? c.LastMessageAt,
                entry.HasUnread
            );
        }).ToList();
    }

    public async Task<List<MessageResponseDto>> GetMessagesAsync(string firebaseUid, Guid conversationId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid)
                   ?? throw new InvalidOperationException("User not found.");

        var convo = await _context.Conversations
            .Include(c => c.StudentProfile)
            .Include(c => c.CompanyProfile)
            .FirstOrDefaultAsync(c => c.Id == conversationId)
            ?? throw new ArgumentException("Conversation not found.");

        var isParticipant = await IsParticipant(user, convo);
        if (!isParticipant)
            throw new InvalidOperationException("You are not allowed to view this conversation.");

        // Mark read across every thread for this student–company pair (legacy duplicates may exist).
        var threadIds = await _context.Conversations
            .Where(c => c.StudentProfileId == convo.StudentProfileId
                        && c.CompanyProfileId == convo.CompanyProfileId)
            .Select(c => c.Id)
            .ToListAsync();

        var unreadFromOthers = await _context.Messages
            .Where(m => threadIds.Contains(m.ConversationId)
                        && m.SenderUserId != user.Id
                        && !m.IsRead)
            .ToListAsync();

        if (unreadFromOthers.Count > 0)
        {
            foreach (var message in unreadFromOthers)
            {
                message.IsRead = true;
            }

            var unreadMessageAlerts = await _context.Notifications
                .Where(n => n.UserId == user.Id
                            && !n.IsRead
                            && n.Type == NotificationType.Message
                            && (convo.OpportunityId == null
                                ? n.RelatedOpportunityId == null
                                : n.RelatedOpportunityId == convo.OpportunityId))
                .ToListAsync();

            foreach (var notification in unreadMessageAlerts)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        var messages = await _context.Messages
            .Include(m => m.SenderUser)
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.SentAt)
            .ToListAsync();

        return messages.Select(m => new MessageResponseDto(
            m.Id,
            m.ConversationId,
            m.SenderUserId,
            m.SenderUser.Email,
            m.Content,
            m.IsRead,
            m.SentAt
        )).ToList();
    }

    public async Task<MessageResponseDto> SendMessageAsync(string firebaseUid, Guid conversationId, SendMessageRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Content))
            throw new ArgumentException("Message content is required.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid)
                   ?? throw new InvalidOperationException("User not found.");

        var convo = await _context.Conversations
            .Include(c => c.StudentProfile)
            .Include(c => c.CompanyProfile)
            .FirstOrDefaultAsync(c => c.Id == conversationId)
            ?? throw new ArgumentException("Conversation not found.");

        var isParticipant = await IsParticipant(user, convo);
        if (!isParticipant)
            throw new InvalidOperationException("You are not allowed to send messages in this conversation.");

        var msg = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderUserId = user.Id,
            Content = dto.Content.Trim(),
            IsRead = false,
            SentAt = DateTime.UtcNow
        };

        _context.Messages.Add(msg);
        convo.LastMessageAt = msg.SentAt;

        Notification? offerResponseNotification = null;
        if (user.Role == UserRole.Student
            && JobOfferResponseMatcher.TryParse(msg.Content, out var offerAccepted))
        {
            offerResponseNotification = await _applicationService.TryApplyJobOfferResponseInConversationAsync(
                firebaseUid,
                conversationId,
                offerAccepted);
        }

        var recipientUserId = user.Role == UserRole.Student
            ? convo.CompanyProfile.UserId
            : convo.StudentProfile.UserId;

        // One unread message alert per conversation thread (not per message).
        var staleMessageAlerts = await _context.Notifications
            .Where(n => n.UserId == recipientUserId
                        && !n.IsRead
                        && n.Type == NotificationType.Message
                        && (convo.OpportunityId == null
                            ? n.RelatedOpportunityId == null
                            : n.RelatedOpportunityId == convo.OpportunityId))
            .ToListAsync();

        foreach (var stale in staleMessageAlerts)
        {
            stale.IsRead = true;
        }

        var messageNotification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = recipientUserId,
            Type = NotificationType.Message,
            Title = "New message",
            Body = "You received a new message.",
            RelatedOpportunityId = convo.OpportunityId,
            RelatedConversationId = conversationId,
            RelatedStudentProfileId = user.Role == UserRole.Student
                ? convo.StudentProfileId
                : null,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.Notifications.Add(messageNotification);

        await _context.SaveChangesAsync();

        var response = new MessageResponseDto(
            msg.Id,
            msg.ConversationId,
            msg.SenderUserId,
            user.Email,
            msg.Content,
            msg.IsRead,
            msg.SentAt
        );

        // Send real-time notification via SignalR
        if (_realtimeNotification != null)
        {
            try
            {
                await _realtimeNotification.NotifyNewMessageAsync(recipientUserId, response);

                var recipient = await _context.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == recipientUserId);
                if (recipient != null && !string.IsNullOrWhiteSpace(recipient.FirebaseUid))
                {
                    var notificationDto = new NotificationResponseDto(
                        messageNotification.Id,
                        messageNotification.Type.ToString(),
                        messageNotification.Title,
                        messageNotification.Body,
                        messageNotification.IsRead,
                        messageNotification.CreatedAt,
                        messageNotification.RelatedOpportunityId,
                        messageNotification.RelatedApplicationId,
                        messageNotification.RelatedConversationId,
                        messageNotification.RelatedStudentProfileId
                    );
                    await _realtimeNotification.NotifyNotificationAsync(recipient.FirebaseUid, notificationDto);
                }

                if (offerResponseNotification != null)
                {
                    var companyUser = await _context.Users.AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Id == offerResponseNotification.UserId);
                    if (companyUser != null && !string.IsNullOrWhiteSpace(companyUser.FirebaseUid))
                    {
                        var offerNotifDto = new NotificationResponseDto(
                            offerResponseNotification.Id,
                            offerResponseNotification.Type.ToString(),
                            offerResponseNotification.Title,
                            offerResponseNotification.Body,
                            offerResponseNotification.IsRead,
                            offerResponseNotification.CreatedAt,
                            offerResponseNotification.RelatedOpportunityId,
                            offerResponseNotification.RelatedApplicationId,
                            offerResponseNotification.RelatedConversationId,
                            offerResponseNotification.RelatedStudentProfileId
                        );
                        await _realtimeNotification.NotifyNotificationAsync(companyUser.FirebaseUid, offerNotifDto);
                    }
                }
            }
            catch
            {
                // Continue even if SignalR fails (fallback to polling)
            }
        }

        return response;
    }

    /// <summary>
    /// Keeps at most one unread "New message" notification per opportunity thread.
    /// </summary>
    private async Task CollapseStaleMessageNotificationsAsync(Guid userId)
    {
        var unreadMessageAlerts = await _context.Notifications
            .Where(n => n.UserId == userId
                        && !n.IsRead
                        && n.Type == NotificationType.Message
                        && n.Title == "New message")
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        var groups = unreadMessageAlerts.GroupBy(n => n.RelatedOpportunityId);
        var changed = false;

        foreach (var group in groups)
        {
            var duplicates = group.Skip(1).ToList();
            if (duplicates.Count == 0)
            {
                continue;
            }

            foreach (var duplicate in duplicates)
            {
                duplicate.IsRead = true;
                changed = true;
            }
        }

        if (changed)
        {
            await _context.SaveChangesAsync();
        }
    }

    private async Task<bool> IsParticipant(User user, Conversation convo)
    {
        if (user.Role == UserRole.Student)
        {
            var student = await _context.StudentProfiles.FirstOrDefaultAsync(s => s.UserId == user.Id);
            return student != null && student.Id == convo.StudentProfileId;
        }

        if (user.Role == UserRole.Company)
        {
            var company = await _context.CompanyProfiles.FirstOrDefaultAsync(c => c.UserId == user.Id);
            return company != null && company.Id == convo.CompanyProfileId;
        }

        return false;
    }
}
