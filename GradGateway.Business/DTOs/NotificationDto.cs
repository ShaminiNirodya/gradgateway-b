namespace GradGateway.Business.DTOs;

public record NotificationResponseDto(
    Guid Id,
    string Type,
    string Title,
    string Body,
    bool IsRead,
    DateTime CreatedAt,
    Guid? RelatedOpportunityId,
    Guid? RelatedApplicationId = null,
    Guid? RelatedConversationId = null,
    Guid? RelatedStudentProfileId = null
);
