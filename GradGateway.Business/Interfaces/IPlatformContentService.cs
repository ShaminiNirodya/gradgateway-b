using GradGateway.Business.DTOs;

namespace GradGateway.Business.Interfaces;

public interface IPlatformContentService
{
    Task<IReadOnlyList<PublicPlatformContentDto>> GetPublishedAsync(
        string? contentType = null,
        string? section = null,
        string? audience = null,
        string? slug = null);

    Task<IReadOnlyList<AdminPlatformContentListItemDto>> GetAdminListAsync(
        string? contentType = null,
        string? section = null,
        string? status = null);

    Task<AdminPlatformContentListItemDto> CreateAsync(AdminCreatePlatformContentDto dto);
    Task<AdminPlatformContentListItemDto> UpdateAsync(Guid id, AdminUpdatePlatformContentDto dto);
    Task DeleteAsync(Guid id);
}
