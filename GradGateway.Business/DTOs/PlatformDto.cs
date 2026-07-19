namespace GradGateway.Business.DTOs;

/// <summary>Anonymous-safe platform flags exposed to the frontend before login.</summary>
public record PublicPlatformSettingsDto(
    bool AllowRegistration,
    bool MaintenanceMode,
    DateTime UpdatedAt);
