namespace GradGateway.Business.DTOs;

public record AdminDashboardDto(
    int TotalStudents,
    int TotalCompanies,
    int TotalProjects,
    decimal HiringRate,
    int TotalUsers,
    int ActiveUsers,
    int SuspendedUsers,
    int StudentAccounts,
    int CompanyAccounts,
    int AdminAccounts,
    int TotalApplications,
    int HiredApplications,
    int SignupsLast7Days,
    int ActiveJobPosts,
    int ExpiredJobPosts,
    int OpenSupportInquiries,
    int TotalSupportInquiries
);

public record AdminUserListItemDto(
    Guid Id,
    string Email,
    string Role,
    bool IsActive,
    DateTime CreatedAt,
    string? DisplayName,
    Guid? StudentProfileId,
    Guid? CompanyProfileId,
    string? StudentUniversity,
    string? StudentDegree
);

public record AdminCompanyListItemDto(
    Guid Id,
    Guid UserId,
    string CompanyName,
    string CompanyEmail,
    string Industry,
    DateTime CreatedAt,
    int ActiveJobCount,
    string UserEmail,
    bool UserIsActive
);

public record AdminPlatformSettingsDto(
    bool AllowRegistration,
    bool MaintenanceMode,
    DateTime UpdatedAt
);

public record AdminUpdatePlatformSettingsDto(
    bool AllowRegistration,
    bool MaintenanceMode
);

public record AdminSetUserActiveDto(bool IsActive);

public record AdminEmailLogItemDto(
    Guid Id,
    string UserEmail,
    string ToEmail,
    string TemplateType,
    string Purpose,
    string Provider,
    string Status,
    string? Error,
    DateTime CreatedAt,
    DateTime? SentAt);
