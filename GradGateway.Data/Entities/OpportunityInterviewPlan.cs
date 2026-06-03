namespace GradGateway.Data.Entities;

public class OpportunityInterviewPlan
{
    public Guid Id { get; set; }
    public Guid OpportunityId { get; set; }
    public Opportunity Opportunity { get; set; } = null!;

    /// <summary>JSON array of UTC date strings (yyyy-MM-dd), sorted ascending.</summary>
    public string TentativeDatesJson { get; set; } = "[]";

    public int DurationMinutes { get; set; } = 60;
    public InterviewMode Mode { get; set; } = InterviewMode.Online;
    public string? MeetingLink { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
