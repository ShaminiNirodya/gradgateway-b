using GradGateway.Business.DTOs;
using GradGateway.Data.Entities;

namespace GradGateway.Business.Interfaces;

public interface IApplicationService
{
    Task<ApplicationResponseDto> ApplyAsync(string firebaseUid, ApplyRequestDto dto);
    Task<List<ApplicationResponseDto>> GetStudentApplicationsAsync(string firebaseUid);
    Task<List<ApplicationResponseDto>> GetCompanyApplicationsAsync(string firebaseUid);
    Task<ApplicationResponseDto> UpdateStatusAsync(string firebaseUid, Guid applicationId, string status);
    Task<ApplicationResponseDto> CreateJobOfferApplicationAsync(
        string firebaseUid,
        Guid studentProfileId,
        string jobTitle,
        string jobType,
        string? compensation,
        string proposalMessage,
        Guid? opportunityId = null);
    Task<ApplicationResponseDto> RespondToJobOfferAsync(
        string firebaseUid,
        Guid conversationId,
        bool accepted,
        Guid? applicationId = null);

    /// <summary>
    /// Updates a pending direct offer when the student accepts or declines in chat.
    /// Returns the company notification when a row was updated; otherwise null.
    /// </summary>
    Task<Notification?> TryApplyJobOfferResponseInConversationAsync(
        string firebaseUid,
        Guid conversationId,
        bool accepted,
        Guid? applicationId = null);

    /// <summary>Syncs direct-offer application status from the student's chat replies. Returns how many rows were updated.</summary>
    Task<int> SyncDirectOfferStatusesFromMessagesAsync(string firebaseUid);
}
