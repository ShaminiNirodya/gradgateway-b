namespace GradGateway.Business.DTOs;

public record CompanyRegistrationDto(
    string Email,
    string FirebaseUid,
    string CompanyName,
    string CompanyEmail,
    string Phone,
    string? Website,
    string Industry,
    string? LogoDataUrl,
    string RecruiterName,
    string RecruiterEmail,
    string RecruiterPhone,
    string Position
);

public record CompanyProfileResponseDto(
    string Email,
    string FirebaseUid,
    string CompanyName,
    string CompanyEmail,
    string Phone,
    string? Website,
    string Industry,
    string? LogoDataUrl,
    string RecruiterName,
    string RecruiterEmail,
    string RecruiterPhone,
    string Position,
    string VerificationStatus,
    string? VerificationRejectionReason
);

public record CompanyPublicOpeningDto(
    Guid Id,
    string Title,
    string Location,
    string OpportunityType,
    string WorkMode,
    DateTime DeadlineAt,
    decimal? MonthlyStipendLkr,
    DateTime CreatedAt
);

public record CompanyPublicProfileDto(
    Guid Id,
    string CompanyName,
    string CompanyEmail,
    string Phone,
    string? Website,
    string Industry,
    string? LogoDataUrl,
    string RecruiterName,
    string RecruiterEmail,
    string RecruiterPhone,
    string Position,
    int ActiveOpeningsCount,
    IReadOnlyList<CompanyPublicOpeningDto> Openings
);
