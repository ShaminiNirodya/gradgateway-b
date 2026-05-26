namespace GradGateway.Business.DTOs;

public record EmailLogTrackRequestDto(
    string ToEmail,
    string TemplateType,
    string Purpose,
    string Status,
    string? Provider,
    string? ProviderMessageId,
    string? PayloadJson,
    string? Error,
    DateTime? SentAt
);

public record EmailLogResponseDto(
    Guid Id,
    string ToEmail,
    string TemplateType,
    string Purpose,
    string Provider,
    string Status,
    string? ProviderMessageId,
    string? PayloadJson,
    string? Error,
    DateTime CreatedAt,
    DateTime? SentAt
);
