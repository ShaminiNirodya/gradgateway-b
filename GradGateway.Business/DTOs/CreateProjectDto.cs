namespace GradGateway.Business.DTOs;

public record CreateProjectImageDto(
    string Url, // Firebase Storage URL
    string MimeType // e.g., "image/png", "image/jpeg"
);

public record CreateProjectDto(
    string Title,
    string Description,
    string TechStack,
    string? RepositoryUrl,
    string? DemoUrl,
    bool IsPublic = true,
    List<CreateProjectImageDto>? Images = null
);

public record UpdateProjectDto(
    string Title,
    string Description,
    string TechStack,
    string? RepositoryUrl,
    string? DemoUrl,
    bool IsPublic = true,
    List<CreateProjectImageDto>? NewImages = null,
    List<Guid>? DeleteImageIds = null
);
