namespace GradGateway.Data.Entities;

public enum OpportunityType
{
    Internship,
    GraduateRole,
    PartTime,
    Contract
}

public enum WorkMode
{
    Onsite,
    Hybrid,
    Remote
}

public class Opportunity
{
    public Guid Id { get; set; }
    public Guid CompanyProfileId { get; set; }
    public CompanyProfile CompanyProfile { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public OpportunityType OpportunityType { get; set; }
    public WorkMode WorkMode { get; set; }
    public string Location { get; set; } = string.Empty;
    public string RequiredSkills { get; set; } = string.Empty;
    public decimal? MonthlyStipendLkr { get; set; }
    public DateTime DeadlineAt { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
