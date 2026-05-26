namespace GradGateway.Data.Entities;

public class CompanyProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string CompanyName { get; set; } = string.Empty;
    public string CompanyEmail { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Website { get; set; }
    public string Industry { get; set; } = string.Empty;
    public string? LogoDataUrl { get; set; }

    public string RecruiterName { get; set; } = string.Empty;
    public string RecruiterEmail { get; set; } = string.Empty;
    public string RecruiterPhone { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
