using GradGateway.Business.DTOs;

namespace GradGateway.Business.Interfaces;

public interface IOpportunityService
{
    Task<OpportunityResponseDto> CreateOpportunityAsync(string firebaseUid, CreateOpportunityRequestDto dto);
    Task<List<OpportunityResponseDto>> GetActiveOpportunitiesAsync();
    Task<List<OpportunityResponseDto>> GetCompanyOpportunitiesAsync(string firebaseUid);
    Task<OpportunityResponseDto?> GetOpportunityByIdAsync(Guid id);
    Task<ScheduleInterviewsResultDto> ScheduleInterviewsAsync(string firebaseUid, Guid opportunityId, ScheduleInterviewsRequestDto dto);
}
