using GradGateway.Business.Interfaces;

namespace GradGateway.Api.Background;

public class DeadlineNotificationBackgroundService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DeadlineNotificationBackgroundService> _logger;

    public DeadlineNotificationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<DeadlineNotificationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IDeadlineNotificationProcessor>();
                var count = await processor.ProcessExpiredOpportunityDeadlinesAsync();
                if (count > 0)
                {
                    _logger.LogInformation("Sent {Count} job deadline notification(s).", count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Deadline notification background check failed.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }
}
