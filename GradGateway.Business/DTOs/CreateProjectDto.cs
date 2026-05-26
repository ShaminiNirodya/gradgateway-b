namespace GradGateway.Business.DTOs;

public record CreateProjectDto(
    string Title,
    string Description,
    string TechStack,
    string? RepositoryUrl,
    string? DemoUrl,
    bool IsPublic = true
);
