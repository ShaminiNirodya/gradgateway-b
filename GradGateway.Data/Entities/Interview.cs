namespace GradGateway.Data.Entities;

public enum InterviewStatus
{
    Scheduled,
    Completed,
    Cancelled,
    NoShow
}

public enum InterviewMode
{
    Online,
    Onsite,
    Phone
}

public class Interview
{
    public Guid Id { get; set; }

    public Guid ApplicationId { get; set; }
    public Application Application { get; set; } = null!;

    public DateTime ScheduledAt { get; set; }
    public InterviewMode Mode { get; set; } = InterviewMode.Online;
    public string? MeetingLink { get; set; }
    public string? Location { get; set; }
    public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
