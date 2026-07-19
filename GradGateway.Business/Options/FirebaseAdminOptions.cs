namespace GradGateway.Business.Options;

public class FirebaseAdminOptions
{
    public const string SectionName = "Firebase";

    public string ProjectId { get; set; } = string.Empty;
    public string? ServiceAccountPath { get; set; }
}
