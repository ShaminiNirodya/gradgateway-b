namespace GradGateway.Data.Entities;

public enum ApplicationStatus
{
    Pending,
    Shortlisted,
    Rejected,
    Hired,
    OfferSent,
    Interviewed,
    /// <summary>Student accepted a direct job offer (open for interview).</summary>
    OfferAccepted
}

public class Application
{
    public Guid Id { get; set; }
    public Guid? OpportunityId { get; set; }
    public Opportunity? Opportunity { get; set; }

    public Guid StudentProfileId { get; set; }
    public StudentProfile StudentProfile { get; set; } = null!;

    public Guid? CompanyProfileId { get; set; }
    public CompanyProfile? CompanyProfile { get; set; }

    public string? CoverLetter { get; set; }
    public string? JobTitle { get; set; }
    public string? JobType { get; set; }
    public string? Compensation { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>When the current interview plan was last sent to this applicant in chat.</summary>
    public DateTime? InterviewPlanNotifiedAt { get; set; }
}
