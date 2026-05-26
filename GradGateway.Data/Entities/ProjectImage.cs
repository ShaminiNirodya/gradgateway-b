namespace GradGateway.Data.Entities;

public class ProjectImage
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string ImageUrl { get; set; } = string.Empty; // Firebase Storage URL
    public string? ImageMimeType { get; set; } // e.g., "image/png", "image/jpeg"
    public int DisplayOrder { get; set; } // Order in which images are displayed
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
