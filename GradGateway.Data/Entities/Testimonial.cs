namespace GradGateway.Data.Entities;

public class Testimonial
{
    public Guid Id { get; set; }
    public string Quote { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorRole { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public int SortOrder { get; set; }
    public string? SubmitterEmail { get; set; }
    public string? SubmitterRole { get; set; }
    public Guid? SubmittedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
