namespace GradGateway.Data.Entities;

public class CompanyTeamMember
{
    public Guid Id { get; set; }

    public Guid CompanyProfileId { get; set; }
    public CompanyProfile CompanyProfile { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    // Pending | Active | Removed
    public string Status { get; set; } = "Pending";

    public string InvitationToken { get; set; } = string.Empty;
    public DateTime InvitationExpiresAt { get; set; }
    public DateTime InvitedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedAt { get; set; }

    public Guid InvitedByUserId { get; set; }
    public User InvitedByUser { get; set; } = null!;
}
