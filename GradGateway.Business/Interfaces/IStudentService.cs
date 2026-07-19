using GradGateway.Business.DTOs;

namespace GradGateway.Business.Interfaces;

public interface IStudentService
{
    Task<StudentProfileResponseDto> RegisterOrUpdateStudentAsync(StudentRegistrationDto dto);
    Task<StudentProfileResponseDto?> GetStudentByFirebaseUidAsync(string firebaseUid);
    Task<List<StudentDirectoryItemDto>> GetStudentDirectoryAsync(string? query);
    Task<PagedResultDto<StudentDirectoryItemDto>> SearchStudentDirectoryAsync(StudentDirectorySearchRequest request);
    Task<StudentDirectoryItemDto?> GetStudentDirectoryItemByProfileIdAsync(Guid studentProfileId);
    Task<List<StudentSkillDto>> GetMySkillsAsync(string firebaseUid);
    Task<StudentSkillDto> AddSkillAsync(string firebaseUid, AddStudentSkillDto dto);
    Task RemoveSkillAsync(string firebaseUid, Guid studentSkillId);
    Task<List<StudentInterviewDto>> GetMyInterviewsAsync(string firebaseUid);
}
