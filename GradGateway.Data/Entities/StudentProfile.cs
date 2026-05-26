namespace GradGateway.Data.Entities;

public class StudentProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? PhotoDataUrl { get; set; }

    public string University { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public int GradYear { get; set; }
    public decimal Gpa { get; set; }
    public string? CertificationsJson { get; set; }
    public string? AwardsJson { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
