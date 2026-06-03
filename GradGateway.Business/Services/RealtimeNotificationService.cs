using GradGateway.Business.Interfaces;

namespace GradGateway.Business.Services;

public class RealtimeNotificationService : IRealtimeNotificationService
{
    public Func<Guid, object, Task>? SendMessageFunc { get; set; }
    public Func<Guid, object, Task>? SendConversationUpdateFunc { get; set; }
    public Func<string, object, Task>? SendNotificationFunc { get; set; }

    public async Task NotifyNewMessageAsync(Guid recipientUserId, object messageData)
    {
        if (SendMessageFunc != null)
        {
            try
            {
                await SendMessageFunc(recipientUserId, messageData);
            }
            catch
            {
                // Silently fail if SignalR is not available
            }
        }
    }

    public async Task NotifyConversationUpdateAsync(Guid recipientUserId, object conversationData)
    {
        if (SendConversationUpdateFunc != null)
        {
            try
            {
                await SendConversationUpdateFunc(recipientUserId, conversationData);
            }
            catch
            {
                // Silently fail if SignalR is not available
            }
        }
    }

    public async Task NotifyNotificationAsync(string firebaseUid, object notificationData)
    {
        if (SendNotificationFunc != null)
        {
            try
            {
                await SendNotificationFunc(firebaseUid, notificationData);
            }
            catch
            {
                // Silently fail if SignalR is not available
            }
        }
    }
}
