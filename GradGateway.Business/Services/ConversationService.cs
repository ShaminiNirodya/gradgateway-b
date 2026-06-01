using GradGateway.Business.DTOs;
using GradGateway.Business.Interfaces;
using GradGateway.Data.Context;
using GradGateway.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GradGateway.Business.Services;

public class ConversationService : IConversationService
{
    private readonly GradGatewayDbContext _context;
    private readonly IRealtimeNotificationService? _realtimeNotification;

    public ConversationService(GradGatewayDbContext context, IRealtimeNotificationService? realtimeNotification = null)
    {
        _context = context;
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

        return new ConversationResponseDto(convo.Id, convo.OpportunityId, other, otherPhoto, string.Empty, convo.LastMessageAt);
    }

    public async Task<List<ConversationResponseDto>> GetMyConversationsAsync(string firebaseUid)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid)
                   ?? throw new InvalidOperationException("User not found.");

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

        // Hide legacy duplicate threads by keeping only the strongest row per student-company pair.
        rows = rows
            .GroupBy(c => new { c.StudentProfileId, c.CompanyProfileId })
            .Select(group => group
                .OrderByDescending(c => lastMessageByConversationId.ContainsKey(c.Id))
                .ThenByDescending(c => lastMessageByConversationId.TryGetValue(c.Id, out var lm) ? lm.SentAt : DateTime.MinValue)
                .ThenByDescending(c => c.LastMessageAt)
                .First())
            .OrderByDescending(c => lastMessageByConversationId.TryGetValue(c.Id, out var lm) ? lm.SentAt : c.LastMessageAt)
            .ToList();

        return rows.Select(c =>
        {
            lastMessageByConversationId.TryGetValue(c.Id, out var lm);
            var other = user.Role == UserRole.Student ? c.CompanyProfile.CompanyName : c.StudentProfile.FullName;
            var otherPhoto = user.Role == UserRole.Student ? c.CompanyProfile.LogoDataUrl : c.StudentProfile.PhotoDataUrl;
            return new ConversationResponseDto(
                c.Id,
                c.OpportunityId,
                other,
                otherPhoto,
                lm?.Content ?? string.Empty,
                lm?.SentAt ?? c.LastMessageAt
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

        var recipientUserId = user.Role == UserRole.Student
            ? convo.CompanyProfile.UserId
            : convo.StudentProfile.UserId;

        _context.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = recipientUserId,
            Type = NotificationType.Message,
            Title = "New message",
            Body = "You received a new message.",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

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
            }
            catch
            {
                // Continue even if SignalR fails (fallback to polling)
            }
        }

        return response;
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
