namespace GradGateway.Business.DTOs;

public record ProjectResponseDto(
    Guid Id,
    Guid StudentProfileId,
    string StudentName,
    string Title,
    string Description,
    string TechStack,
    string? RepositoryUrl,
    string? DemoUrl,
    bool IsPublic,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<ProjectImageDto>? Images = null
);
