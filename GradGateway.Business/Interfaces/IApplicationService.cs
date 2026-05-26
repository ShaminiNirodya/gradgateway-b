using GradGateway.Business.DTOs;

namespace GradGateway.Business.Interfaces;

public interface IApplicationService
{
    Task<ApplicationResponseDto> ApplyAsync(string firebaseUid, ApplyRequestDto dto);
    Task<List<ApplicationResponseDto>> GetStudentApplicationsAsync(string firebaseUid);
    Task<List<ApplicationResponseDto>> GetCompanyApplicationsAsync(string firebaseUid);
    Task<ApplicationResponseDto> UpdateStatusAsync(string firebaseUid, Guid applicationId, string status);
}
