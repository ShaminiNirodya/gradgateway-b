namespace GradGateway.Business.DTOs;

public record ScheduleInterviewsRequestDto(
    /// <summary>Calendar dates as yyyy-MM-dd (preferred).</summary>
    List<string>? TentativeDates,
    DateTime? ScheduledAt,
    int DurationMinutes,
    string Mode,
    string? MeetingLink,
    string? Location,
    string? Notes,
    bool NotifyExistingShortlisted = true
);

public record ScheduleInterviewsResultDto(
    Guid OpportunityId,
    string JobTitle,
    int ShortlistedCount,
    int MessagesSent,
    int InterviewsScheduled,
    bool PlanSaved
);

public record OpportunityInterviewPlanDto(
    Guid OpportunityId,
    IReadOnlyList<string> TentativeDates,
    int DurationMinutes,
    string Mode,
    string? MeetingLink,
    string? Location,
    string? Notes,
    DateTime? UpdatedAt,
    int ShortlistedCount
);
