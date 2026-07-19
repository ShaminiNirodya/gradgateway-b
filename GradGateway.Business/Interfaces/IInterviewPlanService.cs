using GradGateway.Business.DTOs;

namespace GradGateway.Business.Interfaces;

public interface IInterviewPlanService
{
    Task<OpportunityInterviewPlanDto?> GetPlanAsync(string firebaseUid, Guid opportunityId);
    Task<ScheduleInterviewsResultDto> SavePlanAsync(string firebaseUid, Guid opportunityId, ScheduleInterviewsRequestDto dto);
    Task TryNotifyOnShortlistAsync(Guid applicationId, Guid companyUserId);
}
