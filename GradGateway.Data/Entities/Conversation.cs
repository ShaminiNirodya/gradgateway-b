namespace GradGateway.Data.Entities;

public class Conversation
{
    public Guid Id { get; set; }

    public Guid StudentProfileId { get; set; }
    public StudentProfile StudentProfile { get; set; } = null!;

    public Guid CompanyProfileId { get; set; }
    public CompanyProfile CompanyProfile { get; set; } = null!;

    public Guid? OpportunityId { get; set; }
    public Opportunity? Opportunity { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;
}
