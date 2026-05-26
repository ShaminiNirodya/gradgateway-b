namespace GradGateway.Business.DTOs;

public record ApplyRequestDto(
    Guid OpportunityId,
    string? CoverLetter
);

public record UpdateApplicationStatusRequestDto(
    string Status
);

public record ApplicationResponseDto(
    Guid Id,
    Guid OpportunityId,
    Guid StudentProfileId,
    string JobTitle,
    string CompanyName,
    string StudentName,
    string StudentEmail,
    string? CoverLetter,
    string Status,
    DateTime AppliedAt,
    DateTime UpdatedAt
);
