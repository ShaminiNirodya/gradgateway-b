namespace GradGateway.Business.DTOs;

public record SubmitSupportInquiryDto(
    string Name,
    string Email,
    string? Phone,
    string Type,
    string Message,
    string? AttachmentName
);

public record SupportInquiryListItemDto(
    Guid Id,
    string Name,
    string Email,
    string? Phone,
    string InquiryType,
    string Message,
    string? AttachmentName,
    string Status,
    DateTime CreatedAt,
    DateTime? ReviewedAt
);
