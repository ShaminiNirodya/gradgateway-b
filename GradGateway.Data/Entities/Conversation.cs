namespace GradGateway.Data.Entities;

public class Conversation
{
    public Guid Id { get; set; }

    public string Kind { get; set; } = "StudentCompany";

    public Guid? StudentProfileId { get; set; }
    public StudentProfile? StudentProfile { get; set; }

    public Guid? CompanyProfileId { get; set; }
    public CompanyProfile? CompanyProfile { get; set; }

    /// <summary>For admin support threads: the student or company user being messaged.</summary>
    public Guid? SupportTargetUserId { get; set; }
    public User? SupportTargetUser { get; set; }

    public Guid? OpportunityId { get; set; }
    public Opportunity? Opportunity { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;
}
