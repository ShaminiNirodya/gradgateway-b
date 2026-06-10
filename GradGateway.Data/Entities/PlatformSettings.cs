namespace GradGateway.Data.Entities;

/// <summary>Singleton platform configuration row.</summary>
public class PlatformSettings
{
    public Guid Id { get; set; }
    public bool AllowRegistration { get; set; } = true;
    public bool RequireCompanyVerification { get; set; }
    public bool MaintenanceMode { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
