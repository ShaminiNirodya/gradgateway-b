namespace GradGateway.Data.Entities;

public class PlatformContent
{
    public Guid Id { get; set; }
    public string ContentType { get; set; } = "Faq";
    public string Section { get; set; } = "Public";
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? StepsJson { get; set; }
    public string Audiences { get; set; } = "All";
    public string? Category { get; set; }
    public string? Slug { get; set; }
    public string? RelatedLinkHref { get; set; }
    public string? RelatedLinkLabel { get; set; }
    public string Status { get; set; } = "Published";
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
