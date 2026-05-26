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
    string Position
);
