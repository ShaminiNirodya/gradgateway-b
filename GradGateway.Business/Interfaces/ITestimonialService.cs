using GradGateway.Business.DTOs;

namespace GradGateway.Business.Interfaces;

public interface ITestimonialService
{
    Task<IReadOnlyList<PublicTestimonialDto>> GetPublishedAsync(int? limit = null);
    Task<TestimonialListItemDto> SubmitAsync(SubmitTestimonialDto dto, Guid? submittedByUserId);
    Task<PagedResultDto<TestimonialListItemDto>> GetAdminListAsync(string? status, int page, int pageSize);
    Task<TestimonialListItemDto> CreateAdminAsync(AdminCreateTestimonialDto dto);
    Task<TestimonialListItemDto> UpdateAsync(Guid id, AdminUpdateTestimonialDto dto);
    Task<TestimonialListItemDto> SetStatusAsync(Guid id, string status);
    Task DeleteAsync(Guid id);
}
