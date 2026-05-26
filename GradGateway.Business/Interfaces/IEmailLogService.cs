using GradGateway.Business.DTOs;

namespace GradGateway.Business.Interfaces;

public interface IEmailLogService
{
    Task<EmailLogResponseDto> TrackAsync(string firebaseUid, EmailLogTrackRequestDto dto);
    Task<List<EmailLogResponseDto>> GetMyLogsAsync(string firebaseUid, int take = 100);
}
