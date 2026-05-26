namespace GradGateway.Data.Entities;

public class SavedOpportunity
{
    public Guid Id { get; set; }
    public Guid StudentProfileId { get; set; }
    public StudentProfile StudentProfile { get; set; } = null!;

    public Guid OpportunityId { get; set; }
    public Opportunity Opportunity { get; set; } = null!;

    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
}
