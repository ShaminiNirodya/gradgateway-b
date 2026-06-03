using GradGateway.Business.DTOs;
using GradGateway.Business.Interfaces;
using GradGateway.Data.Context;
using GradGateway.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GradGateway.Business;

public static class NotificationRealtimePublisher
{
    public static async Task PushAsync(
        GradGatewayDbContext context,
        IRealtimeNotificationService? realtime,
        Guid userId,
        Notification notification)
    {
        if (realtime == null)
        {
            return;
        }

        var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null || string.IsNullOrWhiteSpace(user.FirebaseUid))
        {
            return;
        }

        var dto = new NotificationResponseDto(
            notification.Id,
            notification.Type.ToString(),
            notification.Title,
            notification.Body,
            notification.IsRead,
            notification.CreatedAt,
            notification.RelatedOpportunityId);

        try
        {
            await realtime.NotifyNotificationAsync(user.FirebaseUid, dto);
        }
        catch
        {
            // Non-blocking
        }
    }
}
