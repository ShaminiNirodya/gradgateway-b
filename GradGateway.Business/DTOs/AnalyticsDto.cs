namespace GradGateway.Business.DTOs;

public record AnalyticsDataPointDto(string Label, int Value, DateTime Date);

public record CompanyAnalyticsDto(
    int TotalApplications,
    int Shortlisted,
    int Interviewed,
    int OffersSent,
    int Hired,
    IReadOnlyList<AnalyticsDataPointDto> ApplicationsByDay,
    IReadOnlyList<AnalyticsDataPointDto> ApplicationsByWeek);
