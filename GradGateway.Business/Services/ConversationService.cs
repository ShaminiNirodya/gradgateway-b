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

        if (user.Role == UserRole.Admin)
        {
            return await StartAdminSupportConversationAsync(user, dto);
        }

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

        var existing = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Kind == ConversationKinds.StudentCompany
                                   && c.StudentProfileId == studentProfileId
                                   && c.CompanyProfileId == companyProfileId);

        var convo = existing;
        if (convo == null)
        {
            convo = new Conversation
            {
                Id = Guid.NewGuid(),
                Kind = ConversationKinds.StudentCompany,
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
            convo.OpportunityId = dto.OpportunityId;
            await _context.SaveChangesAsync();
        }

        var studentName = await _context.StudentProfiles.Where(s => s.Id == convo.StudentProfileId).Select(s => s.FullName).FirstAsync();
        var studentPhoto = await _context.StudentProfiles.Where(s => s.Id == convo.StudentProfileId).Select(s => s.PhotoDataUrl).FirstOrDefaultAsync();
        var companyName = await _context.CompanyProfiles.Where(c => c.Id == convo.CompanyProfileId).Select(c => c.CompanyName).FirstAsync();
        var companyLogo = await _context.CompanyProfiles.Where(c => c.Id == convo.CompanyProfileId).Select(c => c.LogoDataUrl).FirstOrDefaultAsync();
        var other = user.Role == UserRole.Student ? companyName : studentName;
        var otherPhoto = user.Role == UserRole.Student ? companyLogo : studentPhoto;

        return new ConversationResponseDto(
            convo.Id,
            convo.OpportunityId,
            other,
            otherPhoto,
            string.Empty,
            convo.LastMessageAt,
            false,
            ConversationKinds.StudentCompany);
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
            .Include(c => c.SupportTargetUser)
            .OrderByDescending(c => c.LastMessageAt);

        if (user.Role == UserRole.Student)
        {
            var student = await _context.StudentProfiles.FirstOrDefaultAsync(s => s.UserId == user.Id)
                          ?? throw new InvalidOperationException("Student profile not found.");
            query = query.Where(c =>
                (c.Kind == ConversationKinds.StudentCompany && c.StudentProfileId == student.Id)
                || (c.Kind == ConversationKinds.AdminSupport && c.SupportTargetUserId == user.Id));
        }
        else if (user.Role == UserRole.Company)
        {
            var company = await _context.CompanyProfiles.FirstOrDefaultAsync(c => c.UserId == user.Id)
                          ?? throw new InvalidOperationException("Company profile not found.");
            query = query.Where(c =>
                (c.Kind == ConversationKinds.StudentCompany && c.CompanyProfileId == company.Id)
                || (c.Kind == ConversationKinds.AdminSupport && c.SupportTargetUserId == user.Id));
        }
        else if (user.Role == UserRole.Admin)
        {
            query = query.Where(c => c.Kind == ConversationKinds.AdminSupport);
        }
        else
        {
            query = query.Where(c => false);
        }

        var rows = await query.ToListAsync();
        if (rows.Count == 0)
        {
            return [];
        }

        if (user.Role == UserRole.Admin)
        {
            rows = await CollapseAdminSupportDuplicatesAsync(rows);
        }

        var conversationIds = rows.Select(r => r.Id).ToList();
        var lastMessages = await _context.Messages
            .Where(m => conversationIds.Contains(m.ConversationId))
            .GroupBy(m => m.ConversationId)
            .Select(g => g.OrderByDescending(x => x.SentAt).First())
            .ToListAsync();

        var lastMessageByConversationId = lastMessages.ToDictionary(x => x.ConversationId, x => x);
        var unreadSet = await BuildUnreadConversationSetAsync(user, rows, lastMessageByConversationId);

        if (user.Role == UserRole.Admin)
        {
            var supportTargetIds = rows
                .Where(c => c.SupportTargetUserId != null)
                .Select(c => c.SupportTargetUserId!.Value)
                .Distinct()
                .ToList();

            var displayByUserId = await ResolveSupportTargetDisplayBatchAsync(supportTargetIds);

            var adminDtos = new List<ConversationResponseDto>();
            foreach (var c in rows)
            {
                if (c.SupportTargetUserId == null)
                {
                    continue;
                }

                displayByUserId.TryGetValue(
                    c.SupportTargetUserId.Value,
                    out var display);
                var (name, photo, role) = display;

                lastMessageByConversationId.TryGetValue(c.Id, out var lm);
                adminDtos.Add(new ConversationResponseDto(
                    c.Id,
                    null,
                    name,
                    photo,
                    lm?.Content ?? string.Empty,
                    lm?.SentAt ?? c.LastMessageAt,
                    unreadSet.Contains(c.Id),
                    ConversationKinds.AdminSupport,
                    role));
            }

            return adminDtos
                .OrderByDescending(dto => dto.HasUnread)
                .ThenByDescending(dto => dto.LastMessageAt)
                .ToList();
        }

        var studentCompanyRows = rows.Where(c => c.Kind == ConversationKinds.StudentCompany).ToList();
        var adminSupportRows = rows.Where(c => c.Kind == ConversationKinds.AdminSupport).ToList();

        var aggregated = studentCompanyRows
            .GroupBy(c => new { c.StudentProfileId, c.CompanyProfileId })
            .Select(group => MapStudentCompanyGroup(group.ToList(), user, lastMessageByConversationId, unreadSet))
            .ToList();

        var supportDtos = adminSupportRows
            .Select(c => MapPlatformSupportForUser(c, lastMessageByConversationId, unreadSet))
            .ToList();

        return aggregated
            .Concat(supportDtos)
            .OrderByDescending(dto => dto.HasUnread)
            .ThenByDescending(dto => dto.LastMessageAt)
            .ToList();
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

        var threadIds = convo.Kind == ConversationKinds.AdminSupport
            ? [conversationId]
            : await _context.Conversations
                .Where(c => c.Kind == ConversationKinds.StudentCompany
                            && c.StudentProfileId == convo.StudentProfileId
                            && c.CompanyProfileId == convo.CompanyProfileId)
                .Select(c => c.Id)
                .ToListAsync();

        var unreadFromOthers = await _context.Messages
            .Include(m => m.SenderUser)
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
                            && n.RelatedConversationId != null
                            && threadIds.Contains(n.RelatedConversationId.Value))
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
            FormatSenderName(m.SenderUser, user),
            m.Content,
            m.IsRead,
            m.SentAt,
            m.AttachmentUrl,
            m.AttachmentName,
            m.AttachmentType
        )).ToList();
    }

    public async Task<MessageResponseDto> SendMessageAsync(string firebaseUid, Guid conversationId, SendMessageRequestDto dto)
    {
        var hasAttachment = !string.IsNullOrWhiteSpace(dto.AttachmentUrl);
        if (string.IsNullOrWhiteSpace(dto.Content) && !hasAttachment)
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
            Content = (dto.Content ?? string.Empty).Trim(),
            AttachmentUrl = hasAttachment ? dto.AttachmentUrl!.Trim() : null,
            AttachmentName = hasAttachment ? dto.AttachmentName?.Trim() : null,
            AttachmentType = hasAttachment ? dto.AttachmentType?.Trim() : null,
            IsRead = false,
            SentAt = DateTime.UtcNow
        };

        _context.Messages.Add(msg);
        convo.LastMessageAt = msg.SentAt;

        Notification? offerResponseNotification = null;
        if (convo.Kind == ConversationKinds.StudentCompany
            && user.Role == UserRole.Student
            && JobOfferResponseMatcher.TryParse(msg.Content, out var offerAccepted))
        {
            offerResponseNotification = await _applicationService.TryApplyJobOfferResponseInConversationAsync(
                firebaseUid,
                conversationId,
                offerAccepted);
        }

        var response = new MessageResponseDto(
            msg.Id,
            msg.ConversationId,
            msg.SenderUserId,
            FormatSenderName(user, user),
            msg.Content,
            msg.IsRead,
            msg.SentAt,
            msg.AttachmentUrl,
            msg.AttachmentName,
            msg.AttachmentType
        );

        if (convo.Kind == ConversationKinds.AdminSupport)
        {
            await NotifyAdminSupportRecipientsAsync(user, convo, conversationId, response);
        }
        else
        {
            var recipientUserId = user.Role == UserRole.Student
                ? convo.CompanyProfile!.UserId
                : convo.StudentProfile!.UserId;

            await NotifyDirectMessageRecipientAsync(
                user,
                convo,
                conversationId,
                recipientUserId,
                response,
                offerResponseNotification);
        }

        await _context.SaveChangesAsync();
        return response;
    }

    public async Task DeleteConversationAsync(string firebaseUid, Guid conversationId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid)
                   ?? throw new InvalidOperationException("User not found.");

        if (user.Role is not (UserRole.Student or UserRole.Company))
        {
            throw new InvalidOperationException("Only students and companies can delete chats.");
        }

        var convo = await _context.Conversations
            .Include(c => c.StudentProfile)
            .Include(c => c.CompanyProfile)
            .FirstOrDefaultAsync(c => c.Id == conversationId)
            ?? throw new ArgumentException("Conversation not found.");

        if (!await IsParticipant(user, convo))
        {
            throw new InvalidOperationException("You are not allowed to delete this conversation.");
        }

        List<Guid> threadIds;

        if (convo.Kind == ConversationKinds.StudentCompany)
        {
            if (convo.StudentProfileId == null || convo.CompanyProfileId == null)
            {
                throw new InvalidOperationException("Invalid conversation.");
            }

            threadIds = await _context.Conversations
                .Where(c => c.Kind == ConversationKinds.StudentCompany
                            && c.StudentProfileId == convo.StudentProfileId
                            && c.CompanyProfileId == convo.CompanyProfileId)
                .Select(c => c.Id)
                .ToListAsync();
        }
        else if (convo.Kind == ConversationKinds.AdminSupport)
        {
            threadIds = [conversationId];
        }
        else
        {
            throw new InvalidOperationException("This conversation cannot be deleted.");
        }

        var relatedNotifications = await _context.Notifications
            .Where(n => n.RelatedConversationId != null && threadIds.Contains(n.RelatedConversationId.Value))
            .ToListAsync();

        if (relatedNotifications.Count > 0)
        {
            _context.Notifications.RemoveRange(relatedNotifications);
        }

        var conversationsToDelete = await _context.Conversations
            .Where(c => threadIds.Contains(c.Id))
            .ToListAsync();

        _context.Conversations.RemoveRange(conversationsToDelete);
        await _context.SaveChangesAsync();
    }

    private async Task<ConversationResponseDto> StartAdminSupportConversationAsync(User admin, StartConversationRequestDto dto)
    {
        if (dto.StudentProfileId != null && dto.CompanyProfileId != null)
        {
            throw new ArgumentException("Provide either StudentProfileId or CompanyProfileId, not both.");
        }

        if (dto.StudentProfileId == null && dto.CompanyProfileId == null)
        {
            throw new ArgumentException("StudentProfileId or CompanyProfileId is required for admin chat start.");
        }

        Guid supportTargetUserId;
        string otherName;
        string? otherPhoto;
        string supportTargetRole;

        if (dto.StudentProfileId != null)
        {
            var student = await _context.StudentProfiles
                .FirstOrDefaultAsync(s => s.Id == dto.StudentProfileId.Value)
                ?? throw new ArgumentException("Student profile not found.");

            supportTargetUserId = student.UserId;
            otherName = student.FullName;
            otherPhoto = student.PhotoDataUrl;
            supportTargetRole = "Student";
        }
        else
        {
            var company = await _context.CompanyProfiles
                .FirstOrDefaultAsync(c => c.Id == dto.CompanyProfileId!.Value)
                ?? throw new ArgumentException("Company profile not found.");

            supportTargetUserId = company.UserId;
            otherName = company.CompanyName;
            otherPhoto = company.LogoDataUrl;
            supportTargetRole = "Company";
        }

        var existingRows = await _context.Conversations
            .Where(c => c.Kind == ConversationKinds.AdminSupport
                     && c.SupportTargetUserId == supportTargetUserId)
            .OrderByDescending(c => c.LastMessageAt)
            .ToListAsync();

        Conversation convo;
        if (existingRows.Count == 0)
        {
            convo = new Conversation
            {
                Id = Guid.NewGuid(),
                Kind = ConversationKinds.AdminSupport,
                SupportTargetUserId = supportTargetUserId,
                CreatedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow
            };
            _context.Conversations.Add(convo);
            await _context.SaveChangesAsync();
        }
        else
        {
            convo = existingRows[0];
            if (existingRows.Count > 1)
            {
                await MergeAdminSupportDuplicatesAsync(existingRows);
                convo = existingRows[0];
            }
        }

        return new ConversationResponseDto(
            convo.Id,
            null,
            otherName,
            otherPhoto,
            string.Empty,
            convo.LastMessageAt,
            false,
            ConversationKinds.AdminSupport,
            supportTargetRole);
    }

    private async Task<HashSet<Guid>> BuildUnreadConversationSetAsync(
        User user,
        List<Conversation> rows,
        Dictionary<Guid, Message> lastMessageByConversationId)
    {
        var conversationIds = rows.Select(r => r.Id).ToList();
        var adminUserIds = user.Role == UserRole.Admin
            ? []
            : await _context.Users
                .Where(u => u.Role == UserRole.Admin)
                .Select(u => u.Id)
                .ToListAsync();

        var unreadConversationIds = await _context.Messages
            .Where(m => conversationIds.Contains(m.ConversationId)
                        && !m.IsRead
                        && m.SenderUserId != user.Id)
            .Select(m => m.ConversationId)
            .Distinct()
            .ToListAsync();

        if (user.Role is UserRole.Student or UserRole.Company)
        {
            var unreadFromAdmins = await _context.Messages
                .Where(m => conversationIds.Contains(m.ConversationId)
                            && !m.IsRead
                            && adminUserIds.Contains(m.SenderUserId))
                .Select(m => m.ConversationId)
                .Distinct()
                .ToListAsync();

            unreadConversationIds = unreadConversationIds
                .Concat(unreadFromAdmins)
                .Distinct()
                .ToList();
        }

        var unreadSet = unreadConversationIds.ToHashSet();

        foreach (var row in rows)
        {
            if (!lastMessageByConversationId.TryGetValue(row.Id, out var latestMessage))
            {
                continue;
            }

            if (latestMessage.SenderUserId != user.Id && !latestMessage.IsRead)
            {
                unreadSet.Add(row.Id);
            }
        }

        return unreadSet;
    }

    private ConversationResponseDto MapStudentCompanyGroup(
        List<Conversation> group,
        User user,
        Dictionary<Guid, Message> lastMessageByConversationId,
        HashSet<Guid> unreadSet)
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

        var c = displayConversation;
        var lm = latestMessage;
        var other = user.Role == UserRole.Student ? c.CompanyProfile!.CompanyName : c.StudentProfile!.FullName;
        var otherPhoto = user.Role == UserRole.Student ? c.CompanyProfile!.LogoDataUrl : c.StudentProfile!.PhotoDataUrl;

        return new ConversationResponseDto(
            c.Id,
            c.OpportunityId,
            other,
            otherPhoto,
            lm?.Content ?? string.Empty,
            lm?.SentAt ?? c.LastMessageAt,
            hasUnread,
            ConversationKinds.StudentCompany);
    }

    private ConversationResponseDto MapPlatformSupportForUser(
        Conversation c,
        Dictionary<Guid, Message> lastMessageByConversationId,
        HashSet<Guid> unreadSet)
    {
        lastMessageByConversationId.TryGetValue(c.Id, out var lm);
        return new ConversationResponseDto(
            c.Id,
            null,
            PlatformMessaging.SupportDisplayName,
            null,
            lm?.Content ?? string.Empty,
            lm?.SentAt ?? c.LastMessageAt,
            unreadSet.Contains(c.Id),
            ConversationKinds.AdminSupport);
    }

    private async Task<Dictionary<Guid, (string Name, string? Photo, string Role)>> ResolveSupportTargetDisplayBatchAsync(
        IReadOnlyList<Guid> supportTargetUserIds)
    {
        if (supportTargetUserIds.Count == 0)
        {
            return new Dictionary<Guid, (string Name, string? Photo, string Role)>();
        }

        var users = await _context.Users.AsNoTracking()
            .Where(u => supportTargetUserIds.Contains(u.Id))
            .ToListAsync();

        var studentProfiles = await _context.StudentProfiles.AsNoTracking()
            .Where(s => supportTargetUserIds.Contains(s.UserId))
            .ToListAsync();

        var companyProfiles = await _context.CompanyProfiles.AsNoTracking()
            .Where(c => supportTargetUserIds.Contains(c.UserId))
            .ToListAsync();

        var result = new Dictionary<Guid, (string Name, string? Photo, string Role)>();

        foreach (var targetUserId in supportTargetUserIds)
        {
            var targetUser = users.FirstOrDefault(u => u.Id == targetUserId);
            if (targetUser == null)
            {
                result[targetUserId] = ("User", null, "User");
                continue;
            }

            if (targetUser.Role == UserRole.Student)
            {
                var student = studentProfiles.FirstOrDefault(s => s.UserId == targetUserId);
                result[targetUserId] = student != null
                    ? (student.FullName, student.PhotoDataUrl, "Student")
                    : ("Student", null, "Student");
                continue;
            }

            if (targetUser.Role == UserRole.Company)
            {
                var company = companyProfiles.FirstOrDefault(c => c.UserId == targetUserId);
                result[targetUserId] = company != null
                    ? (company.CompanyName, company.LogoDataUrl, "Company")
                    : ("Company", null, "Company");
                continue;
            }

            result[targetUserId] = (targetUser.Email, null, targetUser.Role.ToString());
        }

        return result;
    }

    private async Task<(string Name, string? Photo, string Role)> ResolveSupportTargetDisplayAsync(Guid supportTargetUserId)
    {
        var batch = await ResolveSupportTargetDisplayBatchAsync([supportTargetUserId]);
        return batch.TryGetValue(supportTargetUserId, out var display)
            ? display
            : ("User", null, "User");
    }

    private async Task NotifyAdminSupportRecipientsAsync(
        User sender,
        Conversation convo,
        Guid conversationId,
        MessageResponseDto response)
    {
        if (sender.Role == UserRole.Admin)
        {
            if (convo.SupportTargetUserId == null)
            {
                return;
            }

            await NotifyDirectMessageRecipientAsync(sender, convo, conversationId, convo.SupportTargetUserId.Value, response, null);
            return;
        }

        var adminUsers = await _context.Users
            .Where(u => u.Role == UserRole.Admin && u.IsActive)
            .ToListAsync();

        foreach (var admin in adminUsers)
        {
            var staleMessageAlerts = await _context.Notifications
                .Where(n => n.UserId == admin.Id
                            && !n.IsRead
                            && n.Type == NotificationType.Message
                            && n.RelatedConversationId == conversationId)
                .ToListAsync();

            foreach (var stale in staleMessageAlerts)
            {
                stale.IsRead = true;
            }

            var messageNotification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = admin.Id,
                Type = NotificationType.Message,
                Title = "New support message",
                Body = "A user replied in a support conversation.",
                RelatedConversationId = conversationId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(messageNotification);

            if (_realtimeNotification != null)
            {
                try
                {
                    await _realtimeNotification.NotifyNewMessageAsync(admin.Id, response);

                    if (!string.IsNullOrWhiteSpace(admin.FirebaseUid))
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
                            messageNotification.RelatedStudentProfileId);
                        await _realtimeNotification.NotifyNotificationAsync(admin.FirebaseUid, notificationDto);
                    }
                }
                catch
                {
                    // Continue even if SignalR fails.
                }
            }
        }
    }

    private async Task NotifyDirectMessageRecipientAsync(
        User sender,
        Conversation convo,
        Guid conversationId,
        Guid recipientUserId,
        MessageResponseDto response,
        Notification? offerResponseNotification)
    {
        var staleMessageAlerts = await _context.Notifications
            .Where(n => n.UserId == recipientUserId
                        && !n.IsRead
                        && n.Type == NotificationType.Message
                        && (convo.Kind == ConversationKinds.AdminSupport
                            ? n.RelatedConversationId == conversationId
                            : convo.OpportunityId == null
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
            Title = convo.Kind == ConversationKinds.AdminSupport ? "Message from GradGateway" : "New message",
            Body = convo.Kind == ConversationKinds.AdminSupport
                ? "You received a message from the GradGateway team."
                : "You received a new message.",
            RelatedOpportunityId = convo.OpportunityId,
            RelatedConversationId = conversationId,
            RelatedStudentProfileId = sender.Role == UserRole.Student
                ? convo.StudentProfileId
                : null,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.Notifications.Add(messageNotification);

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
                        messageNotification.RelatedStudentProfileId);
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
                            offerResponseNotification.RelatedStudentProfileId);
                        await _realtimeNotification.NotifyNotificationAsync(companyUser.FirebaseUid, offerNotifDto);
                    }
                }
            }
            catch
            {
                // Continue even if SignalR fails.
            }
        }
    }

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

    private async Task<List<Conversation>> CollapseAdminSupportDuplicatesAsync(List<Conversation> rows)
    {
        var adminRows = rows.Where(c => c.Kind == ConversationKinds.AdminSupport).ToList();
        if (adminRows.Count <= 1)
        {
            return rows;
        }

        var grouped = adminRows
            .Where(c => c.SupportTargetUserId != null)
            .GroupBy(c => c.SupportTargetUserId!.Value)
            .Where(g => g.Count() > 1)
            .ToList();

        if (grouped.Count == 0)
        {
            return rows;
        }

        foreach (var group in grouped)
        {
            var ordered = group.OrderByDescending(c => c.LastMessageAt).ToList();
            await MergeAdminSupportDuplicatesAsync(ordered);
        }

        var duplicateIds = grouped
            .SelectMany(g => g.OrderByDescending(c => c.LastMessageAt).Skip(1).Select(c => c.Id))
            .ToHashSet();

        return rows.Where(c => !duplicateIds.Contains(c.Id)).ToList();
    }

    private async Task MergeAdminSupportDuplicatesAsync(List<Conversation> orderedByRecency)
    {
        if (orderedByRecency.Count <= 1)
        {
            return;
        }

        var primary = orderedByRecency[0];
        var duplicateIds = orderedByRecency.Skip(1).Select(c => c.Id).ToList();

        var messagesToMove = await _context.Messages
            .Where(m => duplicateIds.Contains(m.ConversationId))
            .ToListAsync();

        foreach (var message in messagesToMove)
        {
            message.ConversationId = primary.Id;
        }

        var notificationsToMove = await _context.Notifications
            .Where(n => n.RelatedConversationId != null && duplicateIds.Contains(n.RelatedConversationId.Value))
            .ToListAsync();

        foreach (var notification in notificationsToMove)
        {
            notification.RelatedConversationId = primary.Id;
        }

        var duplicates = await _context.Conversations
            .Where(c => duplicateIds.Contains(c.Id))
            .ToListAsync();

        if (messagesToMove.Count > 0)
        {
            primary.LastMessageAt = messagesToMove.Max(m => m.SentAt);
        }

        _context.Conversations.RemoveRange(duplicates);
        await _context.SaveChangesAsync();
    }

    private async Task<bool> IsParticipant(User user, Conversation convo)
    {
        if (convo.Kind == ConversationKinds.AdminSupport)
        {
            if (user.Role == UserRole.Admin)
            {
                return true;
            }

            return convo.SupportTargetUserId == user.Id;
        }

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

    private static string FormatSenderName(User sender, User viewer)
    {
        if (sender.Role == UserRole.Admin && viewer.Role != UserRole.Admin)
        {
            return PlatformMessaging.SupportDisplayName;
        }

        return sender.Email;
    }
}
