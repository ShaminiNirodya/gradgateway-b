namespace GradGateway.Business.DTOs;

public record PlatformStatsDto(
    int TotalStudents,
    int TotalCompanies,
    int TotalProjects,
    decimal HiringRate
);
