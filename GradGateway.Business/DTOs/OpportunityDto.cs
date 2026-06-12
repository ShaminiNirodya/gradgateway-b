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

public record UpdateOpportunityRequestDto(
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
    PagedResultDto<OpportunityResponseDto> Active,
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
