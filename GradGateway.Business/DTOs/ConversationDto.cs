namespace GradGateway.Business.DTOs;

public record StartConversationRequestDto(
    Guid? OpportunityId,
    Guid? StudentProfileId,
    Guid? CompanyProfileId
);

public record SendMessageRequestDto(
    string Content
);

public record ConversationResponseDto(
    Guid Id,
    Guid? OpportunityId,
    string OtherPartyName,
    string LastMessage,
    DateTime LastMessageAt
);

public record MessageResponseDto(
    Guid Id,
    Guid ConversationId,
    Guid SenderUserId,
    string SenderName,
    string Content,
    bool IsRead,
    DateTime SentAt
);
