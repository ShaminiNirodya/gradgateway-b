using GradGateway.Business.DTOs;

namespace GradGateway.Business.Interfaces;

public interface IAdminService
{
    Task<AdminDashboardDto> GetDashboardAsync();
    Task<AdminAnalyticsDto> GetAnalyticsAsync();
    Task<PagedResultDto<AdminUserListItemDto>> GetUsersAsync(
        string? role, string? search, bool? activeOnly, int page = 1, int pageSize = Pagination.DefaultPageSize);
    Task SetUserActiveAsync(Guid userId, bool isActive);
    Task RemoveUserAsync(Guid userId);
    Task<PagedResultDto<AdminCompanyListItemDto>> GetCompaniesAsync(
        string? status, string? search, int page = 1, int pageSize = Pagination.DefaultPageSize);
    Task<PagedResultDto<SupportInquiryListItemDto>> GetSupportInquiriesAsync(
        string? status, string? inquiryType, string? submitterRole, int page = 1, int pageSize = Pagination.DefaultPageSize);
    Task MarkSupportInquiryReviewedAsync(Guid inquiryId);
    Task DeleteSupportInquiryAsync(Guid inquiryId);
    Task<PagedResultDto<AdminEmailLogItemDto>> GetEmailLogsAsync(
        string? search, string? status, int page = 1, int pageSize = 50);
    Task<AdminPlatformSettingsDto> GetPlatformSettingsAsync();
    Task<AdminPlatformSettingsDto> UpdatePlatformSettingsAsync(AdminUpdatePlatformSettingsDto dto);
    Task EnsureAdminAsync(string firebaseUid);
}
