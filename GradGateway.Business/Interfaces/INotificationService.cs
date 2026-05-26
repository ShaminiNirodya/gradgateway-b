using GradGateway.Business.DTOs;

namespace GradGateway.Business.Interfaces;

public interface INotificationService
{
    Task<List<NotificationResponseDto>> GetMyNotificationsAsync(string firebaseUid);
    Task MarkAsReadAsync(string firebaseUid, Guid notificationId);
}
