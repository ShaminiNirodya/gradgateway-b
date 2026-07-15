namespace GradGateway.Business.DTOs;

public record AnalyticsDataPointDto(string Label, int Value, DateTime Date);

public record AnalyticsCountDto(string Label, int Value);

public record DegreeStudentAnalyticsDto(string Degree, int StudentCount, decimal HiringRate);

public record UniversityStudentAnalyticsDto(
    string University,
    int StudentCount,
    decimal HiringRate,
    IReadOnlyList<DegreeStudentAnalyticsDto> Degrees);

public record CompanyAnalyticsDto(
    int TotalApplications,
    int Shortlisted,
    int Interviewed,
    int OffersSent,
    int Hired,
    IReadOnlyList<AnalyticsDataPointDto> ApplicationsByDay,
    IReadOnlyList<AnalyticsDataPointDto> ApplicationsByWeek);

public record AdminAnalyticsDto(
    int TotalStudents,
    int TotalCompanies,
    int TotalApplications,
    int HiredApplications,
    int ActiveJobPosts,
    int SignupsLast7Days,
    int OpenSupportInquiries,
    int PendingTestimonials,
    int PublishedTestimonials,
    decimal HiringRate,
    IReadOnlyList<AnalyticsDataPointDto> SignupsByWeek,
    IReadOnlyList<AnalyticsDataPointDto> ApplicationsByWeek,
    IReadOnlyList<AnalyticsCountDto> ApplicationsByStatus,
    IReadOnlyList<AnalyticsCountDto> TopIndustries,
    IReadOnlyList<UniversityStudentAnalyticsDto> StudentBreakdownByUniversity);
