namespace GradGateway.Data.Entities;

public enum ApplicationStatus
{
    Pending,
    Shortlisted,
    Rejected,
    Hired
}

public class Application
{
    public Guid Id { get; set; }
    public Guid OpportunityId { get; set; }
    public Opportunity Opportunity { get; set; } = null!;

    public Guid StudentProfileId { get; set; }
    public StudentProfile StudentProfile { get; set; } = null!;

    public string? CoverLetter { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
