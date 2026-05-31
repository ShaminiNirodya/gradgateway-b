namespace GradGateway.Business.DTOs;

public record ScheduleInterviewsRequestDto(
    DateTime ScheduledAt,
    int DurationMinutes,
    string Mode,
    string? MeetingLink,
    string? Location,
    string? Notes
);

public record ScheduleInterviewsResultDto(
    Guid OpportunityId,
    string JobTitle,
    int ShortlistedCount,
    int MessagesSent,
    int InterviewsScheduled
);
