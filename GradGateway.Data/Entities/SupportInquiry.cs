namespace GradGateway.Data.Entities;

public class SupportInquiry
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string InquiryType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? AttachmentName { get; set; }
    public string? SubmitterRole { get; set; }
    public string Status { get; set; } = "Open";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
}
