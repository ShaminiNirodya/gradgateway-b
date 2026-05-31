using GradGateway.Business.Interfaces;

namespace GradGateway.Business.Services;

public class RealtimeNotificationService : IRealtimeNotificationService
{
    // This will be injected with the actual hub context via Program.cs
    public Func<Guid, object, Task>? SendMessageFunc { get; set; }
    public Func<Guid, object, Task>? SendConversationUpdateFunc { get; set; }

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
}
