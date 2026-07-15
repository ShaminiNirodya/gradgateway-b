using GradGateway.Business.DTOs;

namespace GradGateway.Business.Interfaces;

public interface IAcademicCatalogService
{
    Task<IReadOnlyList<CatalogUniversityAdminDto>> GetAdminUniversitiesAsync(bool includeHidden = true);
    Task<IReadOnlyList<CatalogDegreeAdminDto>> GetAdminDegreesAsync(bool includeHidden = true);
    Task<CatalogUniversityDetailDto?> GetAdminUniversityAsync(Guid id);
    Task<CatalogUniversityAdminDto> CreateUniversityAsync(UpsertCatalogUniversityDto dto);
    Task<CatalogUniversityAdminDto> UpdateUniversityAsync(Guid id, UpsertCatalogUniversityDto dto);
    Task DeleteUniversityAsync(Guid id);
    Task<CatalogUniversityAdminDto> SetUniversityActiveAsync(Guid id, bool isActive);
    Task<CatalogUniversityDetailDto> SetUniversityDegreesAsync(Guid id, SetCatalogUniversityDegreesDto dto);

    Task<CatalogDegreeAdminDto> CreateDegreeAsync(UpsertCatalogDegreeDto dto);
    Task<CatalogDegreeAdminDto> UpdateDegreeAsync(Guid id, UpsertCatalogDegreeDto dto);
    Task DeleteDegreeAsync(Guid id);
    Task<CatalogDegreeAdminDto> SetDegreeActiveAsync(Guid id, bool isActive);

    Task<PublicAcademicCatalogDto> GetPublicCatalogAsync();
}
