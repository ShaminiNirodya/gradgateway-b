using GradGateway.Business.DTOs;

namespace GradGateway.Business.Interfaces;

public interface IProjectService
{
    Task<List<ProjectResponseDto>> GetMyProjectsAsync(string firebaseUid);
    Task<List<ProjectResponseDto>> GetProjectsByStudentProfileIdAsync(string firebaseUid, Guid studentProfileId);
    Task<ProjectResponseDto?> GetMyProjectByIdAsync(string firebaseUid, Guid projectId);
    Task<ProjectResponseDto?> GetProjectByIdAsync(Guid projectId);
    Task<ProjectResponseDto> CreateProjectAsync(string firebaseUid, CreateProjectDto dto);
    Task<ProjectResponseDto> UpdateProjectAsync(string firebaseUid, Guid projectId, UpdateProjectDto dto);
    Task<bool> DeleteProjectAsync(string firebaseUid, Guid projectId);
}
