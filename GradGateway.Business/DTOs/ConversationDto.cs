namespace GradGateway.Business.DTOs;

public record StartConversationRequestDto(
    Guid? OpportunityId,
    Guid? StudentProfileId,
    Guid? CompanyProfileId
);

public record SendMessageRequestDto(
    string Content,
    string? AttachmentUrl = null,
    string? AttachmentName = null,
    string? AttachmentType = null
);

public record ConversationResponseDto(
    Guid Id,
    Guid? OpportunityId,
    string OtherPartyName,
    string? OtherPartyPhotoUrl,
    string LastMessage,
    DateTime LastMessageAt,
    bool HasUnread,
    string Kind = "StudentCompany",
    string? SupportTargetRole = null);

public record MessageResponseDto(
    Guid Id,
    Guid ConversationId,
    Guid SenderUserId,
    string SenderName,
    string Content,
    bool IsRead,
    DateTime SentAt,
    string? AttachmentUrl = null,
    string? AttachmentName = null,
    string? AttachmentType = null
);
