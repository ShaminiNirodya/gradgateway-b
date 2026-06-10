using GradGateway.Business.DTOs;

namespace GradGateway.Business.Interfaces;

public interface IAdminService
{
    Task<AdminDashboardDto> GetDashboardAsync();
    Task<IReadOnlyList<AdminUserListItemDto>> GetUsersAsync(string? role, string? search, bool? activeOnly);
    Task SetUserActiveAsync(Guid userId, bool isActive);
    Task RemoveUserAsync(Guid userId);
    Task<IReadOnlyList<AdminCompanyListItemDto>> GetCompaniesAsync(string? status, string? search);
    Task<IReadOnlyList<SupportInquiryListItemDto>> GetSupportInquiriesAsync(string? status, string? inquiryType, string? submitterRole);
    Task MarkSupportInquiryReviewedAsync(Guid inquiryId);
    Task DeleteSupportInquiryAsync(Guid inquiryId);
    Task<AdminPlatformSettingsDto> GetPlatformSettingsAsync();
    Task<AdminPlatformSettingsDto> UpdatePlatformSettingsAsync(AdminUpdatePlatformSettingsDto dto);
    Task EnsureAdminAsync(string firebaseUid);
}
