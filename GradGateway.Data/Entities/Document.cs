namespace GradGateway.Data.Entities;

public class Document
{
    public Guid Id { get; set; }

    public Guid StudentProfileId { get; set; }
    public StudentProfile StudentProfile { get; set; } = null!;

    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public bool IsPublic { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
