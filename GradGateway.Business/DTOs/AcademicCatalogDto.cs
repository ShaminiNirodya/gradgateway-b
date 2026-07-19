namespace GradGateway.Business.DTOs;

public record CatalogUniversityAdminDto(
    Guid Id,
    string Name,
    bool IsActive,
    int SortOrder,
    int DegreeCount,
    DateTime UpdatedAt);

public record CatalogDegreeAdminDto(
    Guid Id,
    string Name,
    bool IsActive,
    int SortOrder,
    int UniversityCount,
    DateTime UpdatedAt);

public record CatalogUniversityDetailDto(
    Guid Id,
    string Name,
    bool IsActive,
    int SortOrder,
    IReadOnlyList<CatalogDegreeLinkDto> Degrees,
    DateTime UpdatedAt);

public record CatalogDegreeLinkDto(
    Guid Id,
    string Name,
    bool IsActive,
    bool OfferingIsActive);

public record UpsertCatalogUniversityDto(
    string Name,
    bool IsActive = true,
    int SortOrder = 0);

public record UpsertCatalogDegreeDto(
    string Name,
    bool IsActive = true,
    int SortOrder = 0);

public record SetCatalogUniversityActiveDto(bool IsActive);

public record SetCatalogDegreeActiveDto(bool IsActive);

public record SetCatalogUniversityDegreesDto(IReadOnlyList<Guid> DegreeIds);

public record PublicAcademicCatalogDto(
    IReadOnlyList<PublicUniversityCatalogDto> Universities,
    IReadOnlyList<string> Degrees);

public record PublicUniversityCatalogDto(
    Guid Id,
    string Name,
    IReadOnlyList<string> Degrees);
