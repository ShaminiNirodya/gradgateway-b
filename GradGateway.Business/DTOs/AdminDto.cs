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
    int PendingCompanyVerifications,
    int ApprovedCompanies,
    int RejectedCompanies,
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
    string VerificationStatus,
    string? VerificationRejectionReason,
    DateTime? VerifiedAt,
    DateTime CreatedAt,
    int ActiveJobCount,
    string UserEmail,
    bool UserIsActive
);

public record AdminPlatformSettingsDto(
    bool AllowRegistration,
    bool RequireCompanyVerification,
    bool MaintenanceMode,
    DateTime UpdatedAt
);

public record AdminUpdatePlatformSettingsDto(
    bool AllowRegistration,
    bool RequireCompanyVerification,
    bool MaintenanceMode
);

public record AdminSetUserActiveDto(bool IsActive);

public record AdminSetCompanyVerificationDto(
    string Status,
    string? RejectionReason
);
