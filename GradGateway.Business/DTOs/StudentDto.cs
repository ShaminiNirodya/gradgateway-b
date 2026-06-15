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
    IReadOnlyList<string>? HackathonsCompetitions = null,
    string? CvUrl = null
);

public record StudentProfileResponseDto(
    Guid StudentProfileId,
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
    IReadOnlyList<string> HackathonsCompetitions,
    string? CvUrl = null
);

public record StudentSkillDto(
    Guid Id,
    string Name,
    string Category,
    string ProficiencyLevel
);

public record AddStudentSkillDto(
    string Name,
    string? Category = null,
    string? ProficiencyLevel = null
);

public record StudentInterviewDto(
    Guid Id,
    DateTime ScheduledAt,
    string Mode,
    string? MeetingLink,
    string? Location,
    string Status,
    string? Notes,
    string JobTitle,
    string CompanyName,
    string? CompanyLogoUrl
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
