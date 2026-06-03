using GradGateway.Business.DTOs;

namespace GradGateway.Business.Interfaces;

public interface IStudentService
{
    Task<StudentProfileResponseDto> RegisterOrUpdateStudentAsync(StudentRegistrationDto dto);
    Task<StudentProfileResponseDto?> GetStudentByFirebaseUidAsync(string firebaseUid);
    Task<List<StudentDirectoryItemDto>> GetStudentDirectoryAsync(string? query);
    Task<StudentDirectoryItemDto?> GetStudentDirectoryItemByProfileIdAsync(Guid studentProfileId);
}
