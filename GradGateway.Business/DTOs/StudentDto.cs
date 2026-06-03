namespace GradGateway.Business.DTOs;

public record StudentRegistrationDto(
    string Email,
    string FirebaseUid,
    string FullName,
    string Phone,
    string? PhotoDataUrl,
    string University,
    string? StudentId,
    string Degree,
    string GradYear,
    int CurrentYear,
    string Gpa,
    string? FieldOfMajor = null,
    string? Availability = "Available Now",
    IReadOnlyList<string>? Certifications = null,
    IReadOnlyList<string>? Awards = null,
    string? CvUrl = null
);

public record StudentProfileResponseDto(
    string Email,
    string FirebaseUid,
    string FullName,
    string Phone,
    string? PhotoDataUrl,
    string University,
    string StudentId,
    string Degree,
    string FieldOfMajor,
    int GradYear,
    int CurrentYear,
    decimal Gpa,
    string Availability,
    IReadOnlyList<string> Certifications,
    IReadOnlyList<string> Awards,
    string? CvUrl = null
);

public record StudentDirectoryItemDto(
    Guid StudentProfileId,
    string FullName,
    string University,
    string Degree,
    string FieldOfMajor,
    int GradYear,
    int CurrentYear,
    decimal Gpa,
    string Email,
    string Skills,
    string? PhotoDataUrl,
    string Availability,
    string? CvUrl = null
);
