using GradGateway.Business.DTOs;
using GradGateway.Business.Helpers;
using GradGateway.Business.Interfaces;
using GradGateway.Data.Context;
using GradGateway.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GradGateway.Business.Services;

public class DeadlineNotificationProcessor : IDeadlineNotificationProcessor
{
    private readonly GradGatewayDbContext _context;
    private readonly IRealtimeNotificationService? _realtimeNotification;

    public DeadlineNotificationProcessor(
        GradGatewayDbContext context,
        IRealtimeNotificationService? realtimeNotification = null)
    {
        _context = context;
        _realtimeNotification = realtimeNotification;
    }

    public async Task<int> ProcessExpiredOpportunityDeadlinesAsync()
    {
        var todaySl = DeadlineClock.TodayDateInSriLanka();

        var expired = await _context.Opportunities
            .Include(o => o.CompanyProfile)
            .ThenInclude(c => c.User)
            .Where(o => !o.DeadlineNotificationSent && o.DeadlineAt.Date < todaySl)
            .ToListAsync();

        if (expired.Count == 0)
        {
            return 0;
        }

        var created = 0;

        foreach (var opportunity in expired)
        {
            var shortlistedCount = await _context.Applications
                .CountAsync(a => a.OpportunityId == opportunity.Id && a.Status == ApplicationStatus.Shortlisted);

            var notification = new Notification
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
            };

            _context.Notifications.Add(notification);
            opportunity.DeadlineNotificationSent = true;
            opportunity.UpdatedAt = DateTime.UtcNow;
            created++;

            if (_realtimeNotification != null && !string.IsNullOrWhiteSpace(opportunity.CompanyProfile.User?.FirebaseUid))
            {
                var dto = new NotificationResponseDto(
                    notification.Id,
                    notification.Type.ToString(),
                    notification.Title,
                    notification.Body,
                    notification.IsRead,
                    notification.CreatedAt,
                    notification.RelatedOpportunityId
                );

                try
                {
                    await _realtimeNotification.NotifyNotificationAsync(
                        opportunity.CompanyProfile.User.FirebaseUid,
                        dto
                    );
                }
                catch
                {
                    // Non-blocking: notifications are persisted and available via REST.
                }
            }
        }

        await _context.SaveChangesAsync();
        return created;
    }
}
