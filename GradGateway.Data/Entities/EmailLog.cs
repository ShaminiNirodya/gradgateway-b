namespace GradGateway.Data.Entities;

public class EmailLog
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string ToEmail { get; set; } = string.Empty;
    public string TemplateType { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string Provider { get; set; } = "FirebaseAuth";
    public string Status { get; set; } = "Simulated";
    public string? ProviderMessageId { get; set; }
    public string? PayloadJson { get; set; }
    public string? Error { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }
}
