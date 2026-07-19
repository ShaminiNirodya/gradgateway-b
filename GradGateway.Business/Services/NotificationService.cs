using GradGateway.Business.DTOs;
using GradGateway.Business.Interfaces;
using GradGateway.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace GradGateway.Business.Services;

public class NotificationService : INotificationService
{
    private readonly GradGatewayDbContext _context;

    public NotificationService(GradGatewayDbContext context)
    {
        _context = context;
    }

    public async Task<List<NotificationResponseDto>> GetMyNotificationsAsync(string firebaseUid)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid)
                   ?? throw new InvalidOperationException("User not found.");

        var rows = await _context.Notifications
            .Where(n => n.UserId == user.Id)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return rows.Select(n => new NotificationResponseDto(
            n.Id,
            n.Type.ToString(),
            n.Title,
            n.Body,
            n.IsRead,
            n.CreatedAt,
            n.RelatedOpportunityId,
            n.RelatedApplicationId,
            n.RelatedConversationId,
            n.RelatedStudentProfileId
        )).ToList();
    }

    public async Task MarkAsReadAsync(string firebaseUid, Guid notificationId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid)
                   ?? throw new InvalidOperationException("User not found.");

        var notif = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == user.Id)
                    ?? throw new ArgumentException("Notification not found.");

        notif.IsRead = true;
        await _context.SaveChangesAsync();
    }
}
