namespace GradGateway.Business.Interfaces;

public interface IRealtimeNotificationService
{
    Task NotifyNewMessageAsync(Guid recipientUserId, object messageData);
    Task NotifyConversationUpdateAsync(Guid recipientUserId, object conversationData);
}
