namespace GradGateway.Business.Interfaces;

public interface IDeadlineNotificationProcessor
{
    /// <summary>
    /// Creates in-app notifications for opportunities whose application deadline has passed
    /// and pushes them to connected clients via SignalR.
    /// </summary>
    Task<int> ProcessExpiredOpportunityDeadlinesAsync();
}
