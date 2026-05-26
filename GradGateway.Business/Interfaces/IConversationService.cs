using GradGateway.Business.DTOs;

namespace GradGateway.Business.Interfaces;

public interface IConversationService
{
    Task<ConversationResponseDto> StartConversationAsync(string firebaseUid, StartConversationRequestDto dto);
    Task<List<ConversationResponseDto>> GetMyConversationsAsync(string firebaseUid);
    Task<List<MessageResponseDto>> GetMessagesAsync(string firebaseUid, Guid conversationId);
    Task<MessageResponseDto> SendMessageAsync(string firebaseUid, Guid conversationId, SendMessageRequestDto dto);
}
