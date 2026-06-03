namespace GradGateway.Business.DTOs;

public record CreateOpportunityRequestDto(
    string Title,
    string Description,
    string OpportunityType,
    string WorkMode,
    string Location,
    string RequiredSkills,
    decimal? MonthlyStipendLkr,
    DateTime DeadlineAt
);

public record StudentOpeningsFeedDto(
    List<OpportunityResponseDto> Active,
    int ExpiredCount
);

public record OpportunityResponseDto(
    Guid Id,
    Guid CompanyProfileId,
    string CompanyName,
    string? CompanyLogoUrl,
    string Title,
    string Description,
    string OpportunityType,
    string WorkMode,
    string Location,
    string RequiredSkills,
    decimal? MonthlyStipendLkr,
    DateTime DeadlineAt,
    bool IsActive,
    DateTime CreatedAt
);
