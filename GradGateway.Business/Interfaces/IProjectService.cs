using GradGateway.Business.DTOs;

namespace GradGateway.Business.Interfaces;

public interface IProjectService
{
    Task<List<ProjectResponseDto>> GetMyProjectsAsync(string firebaseUid);
    Task<ProjectResponseDto?> GetMyProjectByIdAsync(string firebaseUid, Guid projectId);
}
