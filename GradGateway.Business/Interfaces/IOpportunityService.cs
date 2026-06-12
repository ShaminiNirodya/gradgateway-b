using GradGateway.Business.DTOs;

namespace GradGateway.Business.Interfaces;

public interface IOpportunityService
{
    Task<OpportunityResponseDto> CreateOpportunityAsync(string firebaseUid, CreateOpportunityRequestDto dto);
    Task<List<OpportunityResponseDto>> GetActiveOpportunitiesAsync();
    Task<StudentOpeningsFeedDto> GetStudentOpeningsFeedAsync(int page = 1, int pageSize = Pagination.DefaultPageSize);
    Task<int> GetExpiredOpportunitiesCountAsync();
    Task<PagedResultDto<OpportunityResponseDto>> GetCompanyOpportunitiesAsync(
        string firebaseUid, int page = 1, int pageSize = Pagination.DefaultPageSize);
    Task<OpportunityResponseDto?> GetOpportunityByIdAsync(Guid id);
    Task<OpportunityResponseDto> UpdateOpportunityAsync(string firebaseUid, Guid opportunityId, UpdateOpportunityRequestDto dto);
    Task<OpportunityResponseDto> CloseOpportunityAsync(string firebaseUid, Guid opportunityId);
    Task DeleteOpportunityAsync(string firebaseUid, Guid opportunityId);
    Task<ScheduleInterviewsResultDto> ScheduleInterviewsAsync(string firebaseUid, Guid opportunityId, ScheduleInterviewsRequestDto dto);
}
